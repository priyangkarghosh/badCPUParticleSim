using System.Numerics;
using System.Runtime.InteropServices;
using BreadMayhem.Toaster.Extensions;
using BreadMayhem.Toaster.Misc;
using BreadMayhem.Toaster.Rendering;
using SFML.Graphics;

namespace BreadMayhem.Toaster.Physics.Primitives;

public class ParticleManager(int maxParticles)
{
    private int _index;
    private Vector2 _gravity = new Vector2(0, 9.81f);
    private float radius = 1f;
    
    // create the memory versions of the references
    private readonly Memory<Vector2> _positions = new Vector2[maxParticles].AsMemory();
    private readonly Memory<Vector2> _previousPositions = new Vector2[maxParticles].AsMemory();
    
    private readonly Memory<Vector2> _velocities = new Vector2[maxParticles].AsMemory();
    private readonly Memory<Vector2> _preSolveVelocities = new Vector2[maxParticles].AsMemory();

    private readonly Memory<float> _restitutions = new float[maxParticles].AsMemory();
    private readonly Memory<float> _inverseMasses = new float[maxParticles].AsMemory();
    
    //
    private SpatialHash _hash = new SpatialHash(2, 10000);


    public int NumParticles => _index;

    // add a particle to the simulation
    public void AddParticle(Vector2 position, float restitution, float mass)
    {
        _positions.Span[_index] = _previousPositions.Span[_index] = position;
        _preSolveVelocities.Span[_index] = _velocities.Span[_index] = Vector2.Zero;
        _inverseMasses.Span[_index] = mass > 0 ? 1 / mass : 0;
        _restitutions.Span[_index] = restitution;
        _index++;
    }
    
    // integrate all the particles
    public void Update(float dt)
    {
        // get the spans
        var positions = _positions.Span;
        var previousPositions = _previousPositions.Span;
        var velocities = _velocities.Span;
        var preSolveVelocities = _preSolveVelocities.Span;
        var restitutions = _restitutions.Span;
        var inverseMasses = _inverseMasses.Span;

        // reset the hash
        _hash.Preliminaries(NumParticles);
        
        // cache gravity
        var gravStep = _gravity * dt;
        for (var i = 0; i < NumParticles; i++)
        {
            // integrate velocities
            _velocities.Span[i] += gravStep * _inverseMasses.Span[i];
            _preSolveVelocities.Span[i] = _velocities.Span[i];
            
            // integrate positions
            _positions.Span[i] = _previousPositions.Span[i] + _velocities.Span[i] * dt;

            // worlds bounds
            if (_positions.Span[i].X < 10 + radius) _positions.Span[i].X = 10 + radius;
            if (_positions.Span[i].X > 290 - radius) _positions.Span[i].X = 290 - radius;
            if (_positions.Span[i].Y < 10 + radius) _positions.Span[i].Y = 10 + radius;
            if (_positions.Span[i].Y > 290 - radius) _positions.Span[i].Y = 290 - radius;
            
            // populate the hash
            _hash.Populate(_positions.Span[i]);
        }
        
        _hash.GenerateContacts(positions, NumParticles);
        foreach (var (p1, p2, dir, depth) in _hash.Contacts)
        {
            positions[p1] -= dir * depth * 0.5f;
            positions[p2] += dir * depth * 0.5f;
        }
        
        // update particle velocities
        var inverseDt = 1 / dt;
        for (var i = 0; i < NumParticles; i++)
        {
            _velocities.Span[i] = (_positions.Span[i] - _previousPositions.Span[i]) * inverseDt;
            _previousPositions.Span[i] = _positions.Span[i];
        }
        
        foreach (var (contact, p1, p2) in _hash.Contacts)
        {
            var normal = Vector2.Normalize(positions[p2] - positions[p1]);
            var preSolveVelMag = Vector2.Dot(preSolveVelocities[p1] - preSolveVelocities[p2], normal);
            var currentVelMag = Vector2.Dot(velocities[p1] - velocities[p2], normal);

            var restitution = restitutions[p1] + restitutions[p2] * 0.5f;
            var inverseMassSum = inverseMasses[p1] + inverseMasses[p2];

            velocities[p1] += normal * (-currentVelMag - restitution * preSolveVelMag) * inverseMasses[p1] /
                           inverseMassSum;
            velocities[p2] -= normal * (-currentVelMag - restitution * preSolveVelMag) * inverseMasses[p2] /
                              inverseMassSum;
            _hash.ContactPool.Return(contact);
        }
        
        for (var i = 0; i < NumParticles; ++i)
            DrawExtensions.DrawCircle(Panel.instance.Screen, positions[i], 1f, Color.White);
    }
}
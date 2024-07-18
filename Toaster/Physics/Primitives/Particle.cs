using System.Numerics;

namespace BreadMayhem.Toaster.Physics.Primitives;

public class Particle
{
    // position variables
    public Vector2 Position;
    public Vector2 PreviousPosition { get; private set; }
    
    // velocity variables
    public Vector2 Velocity;
    public Vector2 PreSolveVelocity { get; private set; }
    
    // particle characteristics
    public readonly float Radius;
    public readonly float Restitution;
    public float InverseMass { get; private set; }

    public float Mass
    {
        get => _mass;
        set
        {
            _mass = value; 
            InverseMass = 1 / value;
        }
    }

    // private
    private Vector2 _forces;
    private float _mass;

    public Particle(Vector2 position, float mass, float radius, float restitution)
    {
        Position = PreviousPosition = position;
        PreSolveVelocity = Velocity = _forces = Vector2.Zero;
        Restitution = restitution; Radius = radius; Mass = mass;
    }

    public void Integrate(float dt)
    {
        // integrate velocity
        Velocity += _forces * dt * InverseMass;
        PreSolveVelocity = Velocity;
        
        // integrate position
        Position = PreviousPosition + Velocity * dt;
        
        // reset external forces
        _forces.X = 0; _forces.Y = 0;

        if (Position.Y < 10 + Radius)
        {
            Position.Y = 10 + Radius;
            Velocity.Y = -Velocity.Y;
        }
        if (Position.Y > 290 - Radius)
        {
            Position.Y = 290 - Radius;
            Velocity.Y = -Velocity.Y;
        }
        
        if (Position.X < 10 + Radius)
        {
            Position.X = 10 + Radius;
            Velocity.X = -Velocity.Y;
        }
        if (Position.X > 290 - Radius)
        {
            Position.X = 290 - Radius;
            Velocity.X = -Velocity.Y;
        }
    }

    public void CalculateVelocity(float dt)
    {
        Velocity = (Position - PreviousPosition) / dt;
        PreviousPosition = Position;
    }

    public void AddForce(Vector2 force)
    {
        _forces += force;
    }
}
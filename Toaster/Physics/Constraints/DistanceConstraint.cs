using System.Numerics;
using BreadMayhem.Toaster.Physics.Primitives;

namespace BreadMayhem.Toaster.Physics.Constraints;

public class DistanceConstraint : IConstraint
{
    private Particle _a, _b;
    private float _distance;
    private float _stiffness;
    public DistanceConstraint(Particle a, Particle b, float distance, float stiffness)
    {
        _a = a;
        _b = b;
        _distance = distance;
        _stiffness = stiffness;
    }
    
    public void Solve(float dt)
    {
        var dir = _a.Position - _b.Position;
        var currentDistance = dir.Length();
        dir = Vector2.Normalize(dir);
        
        var lambda = -(currentDistance - _distance) / (_a.InverseMass + _b.InverseMass);
        _a.Position += lambda * _a.InverseMass * dir;
        _b.Position -= lambda * _b.InverseMass * dir;
    }
}
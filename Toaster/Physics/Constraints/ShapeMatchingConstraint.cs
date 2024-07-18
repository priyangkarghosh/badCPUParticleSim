using System.Numerics;
using BreadMayhem.Toaster.DataStructures;
using BreadMayhem.Toaster.Extensions;
using BreadMayhem.Toaster.Physics.Primitives;
using BreadMayhem.Toaster.Rendering;
using SFML.Graphics;

namespace BreadMayhem.Toaster.Physics.Constraints;

public class ShapeMatchingConstraint : IConstraint
{
    private Particle[] _members;
    private Vector2[] _restOffsets;
    
    private float inverseTotalMass = 0;
    private Vector2 _restCOM = Vector2.Zero;
    
    public ShapeMatchingConstraint(Particle[] members)
    {
        _members = members;
        
        // sum the positions
        foreach (var p in members)
        {
            _restCOM += p.PreviousPosition * p.Mass;
            inverseTotalMass += p.Mass;
        }
        
        // calculate com
        inverseTotalMass = 1 / inverseTotalMass;
        _restCOM *= inverseTotalMass;
        
        // calculate the rest offsets
        _restOffsets = new Vector2[_members.Length];
        for (var i = 0; i < _members.Length; ++i)
            _restOffsets[i] = members[i].PreviousPosition - _restCOM;
    }

    public void Solve(float dt)
    {
        // calculate the current center of mass
        var currentCOM = Vector2.Zero;
        foreach (var p in _members) currentCOM += p.Position * p.Mass;
        currentCOM *= inverseTotalMass;
        
        // init the covariance matrix
        var covarianceMatrix = Matrix2X2.Zero;
        
        for (var i = 0; i < _members.Length; ++i)
        {
            var restOffset = _restOffsets[i];
            var currentOffset = _members[i].Position - currentCOM;
            
            covarianceMatrix.A += currentOffset.X * restOffset.X;
            covarianceMatrix.B += currentOffset.X * restOffset.Y;
            covarianceMatrix.C += currentOffset.Y * restOffset.X;
            covarianceMatrix.D += currentOffset.Y * restOffset.Y;
        }

        var rotationMatrix = covarianceMatrix + Math.Abs(covarianceMatrix.Determinant()) * covarianceMatrix.Transpose().Inverse();
        rotationMatrix *= 1 / (float)Math.Sqrt(Math.Abs(rotationMatrix.Determinant()));
        
        for (var i = 0; i < _members.Length; ++i)
        {
            var transformed = rotationMatrix * _restOffsets[i] + currentCOM;
            _members[i].Position += transformed - _members[i].Position;
            DrawExtensions.DrawCircle(Panel.instance.Screen, _members[i].Position, 2, Color.Blue);
        }
    }
}
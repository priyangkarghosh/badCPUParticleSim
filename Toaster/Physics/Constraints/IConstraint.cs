using BreadMayhem.Toaster.Physics.Primitives;

namespace BreadMayhem.Toaster.Physics.Constraints;

public interface IConstraint
{
    public void Solve(float dt);
}
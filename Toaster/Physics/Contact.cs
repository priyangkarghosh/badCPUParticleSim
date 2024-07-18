using System.Numerics;
using BreadMayhem.Toaster.Misc;
using BreadMayhem.Toaster.Physics.Primitives;

namespace BreadMayhem.Toaster.Physics;

public struct Contact : IEquatable<Contact>
{
    public int A, B;
    public Vector2 Normal;
    public float Depth;

    public Contact Configure(int a, int b, Vector2 normal, float depth)
    {
        A = a; B = b;
        Normal = normal;
        Depth = depth;
        return this;
    }
    
    public override int GetHashCode() => (A << B) ^ (B << A);
    public static bool operator ==(Contact contact1, Contact contact2) => Equals(contact1, contact2);
    public static bool operator !=(Contact contact1, Contact contact2) => !Equals(contact1, contact2);
    public bool Equals(Contact other) => (A == other.A && B == other.B) || (A == other.B && B == other.A);
    public override bool Equals(object? obj) => obj is Contact contact && Equals(contact);
    
    public void Deconstruct(out Contact c, out int a, out int b)
    {
        c = this; a = A; b = B;
    }

    public void Deconstruct(out int a, out int b, out Vector2 normal, out float depth)
    {
        a = A; b = B;
        normal = Normal;
        depth = Depth;
    }
}
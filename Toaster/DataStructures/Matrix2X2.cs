using System.Numerics;

namespace BreadMayhem.Toaster.DataStructures;

public record struct Matrix2X2(float A, float B, float C, float D)
{
    public static Matrix2X2 Zero => new Matrix2X2(0, 0, 0, 0);
    public static Matrix2X2 Identity => new Matrix2X2(1, 0, 0, 1);
    public static Matrix2X2 One => new Matrix2X2(1, 1, 1, 1);
    
    public float Determinant() => A * D - B * C;
    public Matrix2X2 Transpose() => this with {C=B, B=C};
    public Matrix2X2 Inverse() => 1 / Determinant() * new Matrix2X2(D, -B, -C, A);
    public override string ToString() => $"[{A},{B}] [{C}, {D}]";
    
    // operators
    public static Matrix2X2 operator +(Matrix2X2 a) => a;
    public static Matrix2X2 operator -(Matrix2X2 a) => new(-a.A, -a.B, -a.C, -a.D);

    public static Matrix2X2 operator +(Matrix2X2 a, Matrix2X2 b)
        => new(a.A + b.A, a.B + b.B, a.C + b.C, a.D + b.D);
    
    public static Matrix2X2 operator -(Matrix2X2 a, Matrix2X2 b)
        => new(a.A - b.A, a.B - b.B, a.C - b.C, a.D - b.D);
    
    public static Matrix2X2 operator *(Matrix2X2 a, Matrix2X2 b)
        => new(a.A * b.A + a.B * b.C, a.A * b.B + a.B * b.D,
            a.C * b.A + a.D * b.C, a.C * b.B + a.D * b.D);
    
    public static Matrix2X2 operator *(Matrix2X2 a, float b)
        => new(a.A * b, a.B * b, a.C * b, a.D * b);
    public static Matrix2X2 operator *(float a, Matrix2X2 b)
        => new(a * b.A, a * b.B, a * b.C, a * b.D);
    
    public static Vector2 operator *(Vector2 a, Matrix2X2 b)
        => new(a.X * b.A + a.Y * b.B, a.X * b.C + a.Y * b.D);
    public static Vector2 operator *(Matrix2X2 a, Vector2 b)
        => new(a.A * b.X + a.B * b.Y, a.C * b.X + a.D * b.Y);
}
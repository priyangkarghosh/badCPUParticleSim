using System.Numerics;
using SFML.Graphics;
using SFML.System;

namespace BreadMayhem.Toaster.Extensions;

public static class DrawExtensions
{
    private static CircleShape _cshape = new CircleShape();
    public static void DrawCircle(RenderWindow window, Vector2 pos, float radius, Color color)
    {
        _cshape.Radius = radius;
        _cshape.FillColor = color;
        _cshape.Position = new Vector2f(pos.X, pos.Y);
        window.Draw(_cshape);  
    }
    
    private static RectangleShape _rshape = new RectangleShape();
    public static void DrawRect(RenderWindow window, Vector2 pos, Vector2 size, Color color)
    {
        _rshape.FillColor = color;
        _rshape.Position = new Vector2f(pos.X, pos.Y);
        _rshape.Size = new Vector2f(size.X, size.Y);
        window.Draw(_rshape);  
    }
}
using System.Numerics;
using BreadMayhem.Toaster.Physics.Primitives;
using BreadMayhem.Toaster.Rendering;
using SFML.Graphics;

var rng = new Random(3);
var panel = new Panel((300, 300), (900, 900), Color.Black, fpsCap:60);
var pm = new ParticleManager(10000);
for (var i = 0; i < 8000; ++i)
    pm.AddParticle(new Vector2(rng.Next(10, 280), rng.Next(10, 200)), 0.4f, rng.NextSingle());

while (panel.Screen.IsOpen)
{
    pm.Update(panel.DeltaTime);
    panel.Screen.DispatchEvents();
    panel.Update();
}

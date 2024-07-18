using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Numerics;
using BreadMayhem.Toaster.Misc;
using BreadMayhem.Toaster.Physics.Primitives;

namespace BreadMayhem.Toaster.Physics;

public class SpatialHash(float spacing, int bufferSize)
{
    // list to store query results
    public HashSet<Contact> Contacts = [];
    public ObjectPool<Contact> ContactPool = new(40000);

    // create the
    private readonly int _tableSize = bufferSize * 2;
    private readonly int[] _cells = new int[bufferSize * 2 + 1];
    private readonly List<int> _entries = new(bufferSize);
    
    // store neighbour checks
    private static (int, int)[] _neighbours = [(-1, -1), (0, -1), (1, -1), (-1, 0), (0, 0), (1, 0), (-1, 1), (0, 1), (1, 1)];

    public void Preliminaries(int numParticles)
    {
        // reset the counts for all cells to 0
        Array.Fill(_cells, 0);
        
        // add more entries to the particles list if necessary
        var newEntries = numParticles - _entries.Count;
        if (newEntries > 0) for (var i = 0; i < newEntries; ++i)
            _entries.Add(0);
    }
    
    // assume that this will be called by each particle at some point
    public void Populate(Vector2 position) => _cells[CellIndex(position)]++;
    public void Populate(Particle particle) => Populate(particle.Position);

    public void GenerateContacts(Span<Vector2> positions, int numParticles)
    {
        // calculate the partial sums for the array
        var cellSpan = new Span<int>(_cells);
        for (var i = 1; i < _tableSize; i++)
            cellSpan[i] += cellSpan[i - 1];
        
        // store the indices of each particle
        for (var i = 0; i < numParticles; i++)
        {
            var cellIndex = CellIndex(positions[i]);
            _entries[--cellSpan[cellIndex]] = i;
        }
        
        Contacts.Clear();
        Contacts.EnsureCapacity(positions.Length * 9);
        
        var combinedRadius = 2;
        for (var a = 0; a < numParticles; a++)
        {
            var (x, y) = IntCoords(positions[a]);

            foreach (var (dx, dy) in _neighbours)
            {
                var cellIndex = CellIndex(x + dx, y + dy);
                var bucketStart = _cells[cellIndex];
                var bucketEnd = _cells[cellIndex + 1];

                for (var i = bucketStart; i < bucketEnd; i++)
                {
                    var b = _entries[i];
                    if (a == b) continue;

                    // calculate the direction between both particles
                    var direction = positions[b] - positions[a];

                    // in order to determine whether particles are colliding
                    var distanceSquared = direction.LengthSquared();

                    // check if the particles are actually colliding
                    if (distanceSquared <= 0 || distanceSquared >= combinedRadius * combinedRadius)
                        continue;

                    var distance = (float)Math.Sqrt(distanceSquared);
                    Contacts.Add(ContactPool.Get().Configure(
                        a, b, direction / distance, combinedRadius - distance)
                    );
                }
            }
        }
    }

    public IEnumerable<int> Query(Vector2 position, int queryDistance)
    {
        var intCoord = IntCoords(position.X, position.Y);
        var minBound = (intCoord.Item1 - queryDistance, intCoord.Item2 - queryDistance);
        var maxBound = (intCoord.Item1 + queryDistance, intCoord.Item2 + queryDistance);

        for (var x = minBound.Item1; x <= maxBound.Item1; x++)
        {
            for (var y = minBound.Item2; y <= maxBound.Item2; y++)
            {
                var cellIndex = CellIndex(x, y);
                var bucketStart = _cells[cellIndex];
                var bucketEnd = _cells[cellIndex + 1];

                for (var i = bucketStart; i < bucketEnd; i++)
                    yield return _entries[i];
            }
        }
    }

    private int CellIndex(Vector2 position) => HashFunction(IntCoords(position.X, position.Y));
    private int CellIndex(int x, int y) => HashFunction(x, y);

    private (int, int) IntCoords(float x, float y) => ((int)Math.Floor(x / spacing), (int)Math.Floor(y / spacing));
    private (int, int) IntCoords(Vector2 position) => IntCoords(position.X, position.Y);

    // hashes
    private int HashFunction(int x, int y) => Math.Abs((x * 92837111) ^ (y * 689287499)) % _tableSize;
    private int HashFunction((int, int) intPos) => HashFunction(intPos.Item1, intPos.Item2);
}
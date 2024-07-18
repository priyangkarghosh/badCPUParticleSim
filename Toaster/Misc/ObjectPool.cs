namespace BreadMayhem.Toaster.Misc;

public record ObjectPool<T>(int MaxSize) where T : new()
{
    private readonly Stack<T> _stack = new();

    public T Get() => _stack.TryPop(out var item) ? item : new T();
    public void Return(T item) { if (_stack.Count < MaxSize) _stack.Push(item); }
}
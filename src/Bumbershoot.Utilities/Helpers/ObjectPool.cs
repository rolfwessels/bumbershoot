using System;
using System.Threading;

namespace Bumbershoot.Utilities.Helpers;

public class ObjectPool<T>
{
    private readonly T[] _generalUnitOfWorks;
    private readonly Func<T> _getGeneralUnitOfWork;
    private readonly int _size;
    private int _counter;

    public ObjectPool(Func<T> getGeneralUnitOfWork, int size = 4)
    {
        _getGeneralUnitOfWork = getGeneralUnitOfWork;
        _size = size;
        _generalUnitOfWorks = new T[_size];
        _counter = 0;
    }

    public T Get()
    {
        var increment = Interlocked.Increment(ref _counter);
        var indexKey = increment % _size;
        return _generalUnitOfWorks[indexKey] != null
            ? _generalUnitOfWorks[indexKey]
            : _generalUnitOfWorks[indexKey] = _getGeneralUnitOfWork();
    }
}
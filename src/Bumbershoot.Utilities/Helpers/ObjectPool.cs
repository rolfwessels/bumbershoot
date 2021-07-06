using System;
using System.Threading;

namespace Bumbershoot.Utilities.Helpers
{
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
            var index0 = _counter % _size;
            Interlocked.Increment(ref _counter);
            return _generalUnitOfWorks[index0] != null
                ? _generalUnitOfWorks[index0]
                : _generalUnitOfWorks[index0] = _getGeneralUnitOfWork();
        }
    }
}
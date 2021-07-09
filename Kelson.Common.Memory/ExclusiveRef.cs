using System;

namespace Kelson.Common.Memory
{
    public unsafe ref struct ExclusiveRef<T> where T : class
    {
        private T? _value;

        public ExclusiveRef(delegate*<T> value) => _value = value();

        public ExclusiveRef(in T value) => _value = value;

        public void Borrow(delegate*<in T, void> map)
        {
            T t;
            (t, _value) = (_value!, default);
            map(in t);
            _value = t;
        }

        public R Borrow<R>(delegate*<in T, R> map)
        {
            T t;
            (t, _value) = (_value!, default);
            R result = map(in t);
            _value = t;
            return result;
        }

        public R Borrow<U, R>(in U u, delegate*<in T, in U, R> map)
        {
            T t;
            (t, _value) = (_value!, default);
            R result = map(in t, in u);
            _value = t;
            return result;
        }

        public void Dispose() => _value = default;
    }

    public unsafe ref struct Movable<T> where T : notnull
    {
        private T? _value;

        public Movable(delegate*<T> value) => _value = value();

        public T Take()
        {
            T t = _value!;
            _value = default;
            return t;
        }

        public void Dispose() => _value = default;
    }
}
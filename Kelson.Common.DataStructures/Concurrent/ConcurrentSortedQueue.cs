using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Kelson.Common.DataStructures.Concurrent
{
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
    public class ConcurrentSortedQueue<T> where T : notnull, IComparable<T>
    {
        private readonly object key = new();
        private ImmutableSortedSet<T> values = ImmutableSortedSet<T>.Empty;

        public int Count => values.Count;

        public ConcurrentSortedQueue()
        {

        }

        public ConcurrentSortedQueue(T[] initialData)
        {
            var builder = values.ToBuilder();
            for (int i = 0; i < initialData.Length; i++)
                builder.Add(initialData[i]);
            values = builder.ToImmutable();
        }

        public (bool found, T item) TryTakeMin()
        {
            lock (key)
            {
                if (values.Count > 0)
                {
                    var item = values.Min!;
                    values = values.Remove(item);
                    return (true, item);
                }
            }
            return (false, default);
        }

        public (bool found, T item) TryPeekMin()
        {
            lock (key)
            {
                return (values.Count > 0, values.Min);
            }
        }

        public (bool found, T item) TryTakeMax()
        {
            lock (key)
            {
                if (values.Count > 0)
                {
                    var item = values.Max!;
                    values = values.Remove(item);
                    return (true, item);
                }
            }
            return (false, default);
        }

        public (bool found, T item) TryPeekMax()
        {
            lock (key)
            {
                return (values.Count > 0, values.Max);
            }
        }

        public ConcurrentSortedQueue<T> Add(T value)
        {
            lock (key)
            {
                values = values.Add(value);
            }
            return this;
        }

        public ConcurrentSortedQueue<T> AddAll(T value, params T[] more)
        {
            lock (key)
            {
                var builder = values.ToBuilder();
                builder.Add(value);
                for (int i = 0; i < more.Length; i++)
                    builder.Add(more[i]);
                values = builder.ToImmutable();

#if DEBUG
                HashSet<T> existing = new();
                for (int i = 0; i < values.Count; i++)
                {
                    if (existing.Contains(values[i]))
                        throw new InvalidOperationException("Duplicate item found in set");
                    existing.Add(values[i]);
                }
#endif
            }
            return this;
        }
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

        public override string ToString()
        {
            var data = values;
            return $"{data.Count}:[{string.Join(", ", data)}]";
        }
    }
}

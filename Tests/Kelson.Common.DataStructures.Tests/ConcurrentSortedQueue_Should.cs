using Kelson.Common.DataStructures.Concurrent;
using System.Collections.Immutable;

namespace Kelson.Common.DataStructures.Tests
{
    public class ConcurrentSortedQueue_Should
    {
        [Theory]
        [MemberData(nameof(SetsWithMinAndNextMinData))]
        public void TakeMinItem(ConcurrentSortedQueue<int> queue, int min, int nextMin)
        {
            int initialCount = queue.Count;
            var (found, first) = queue.TryTakeMin();
            var (_, second) = queue.TryPeekMin();
            found.Should().BeTrue();
            queue.Count.Should().Be(initialCount - 1);
            first.Should().Be(min);
            second.Should().Be(nextMin);
        }

        [Theory]
        [MemberData(nameof(SetsWithMaxAndNextMaxData))]
        public void TakeMaxItem(ConcurrentSortedQueue<int> queue, int max, int nextMax)
        {
            int initialCount = queue.Count;
            var (found, first) = queue.TryTakeMax();
            var (_, second) = queue.TryPeekMax();
            found.Should().BeTrue();
            queue.Count.Should().Be(initialCount - 1);
            first.Should().Be(max);
            second.Should().Be(nextMax);
        }


        public static IEnumerable<object[]> SetsWithMinAndNextMinData
        {
            get
            {
                var (first, second, third) = (0, 1, 2);
                yield return new object[] { new ConcurrentSortedQueue<int>().AddAll(first, second, third), first, second };

                for (int i = 0; i < 20; i++)
                {
                    var random = new Random();
                    var (r, m, nm, _, _) = RandomDateTimeSet(random, 5, 10, (i) => i >> 6);
                    var set = new ConcurrentSortedQueue<int>(r);
                    yield return new object[] { set, m, nm };
                }

                for (int i = 0; i < 20; i++)
                {
                    var random = new Random();
                    var (r, m, nm, _, _) = RandomDateTimeSet(random, 100, 250, (i) => i);
                    var set = new ConcurrentSortedQueue<int>(r);
                    yield return new object[] { set, m, nm };
                }
            }
        }

        public static IEnumerable<object[]> SetsWithMaxAndNextMaxData
        {
            get
            {
                var (first, second, third) = (0, 1, 2);
                yield return new object[] { new ConcurrentSortedQueue<int>().AddAll(first, second, third), third, second };

                for (int i = 0; i < 20; i++)
                {
                    var random = new Random();
                    var (r, _, _, w, nw) = RandomDateTimeSet(random, 5, 10, (i) => i >> 6);
                    var set = new ConcurrentSortedQueue<int>(r);
                    yield return new object[] { set, w, nw };
                }

                for (int i = 0; i < 20; i++)
                {
                    var random = new Random();
                    var (r, _, _, w, nw) = RandomDateTimeSet(random, 100, 250, (i) => i);
                    var set = new ConcurrentSortedQueue<int>(r);
                    yield return new object[] { set, w, nw };
                }
            }
        }

        private static (T[], T min, T nextMin, T max, T nextMax) RandomDateTimeSet<T>(Random rng, int minCount, int maxCount, Func<int, T> factory) where T : IComparable<T>
        {
            var center = factory(0);
            int count = rng.Next(minCount, maxCount);
            var results = new T[count];
            var found = ImmutableSortedSet<T>.Empty.Add(center).ToBuilder();
            results[0] = center;
            for (int i = 1; i < count;)
            {
                int nextOffsetMs = (rng.Next() - (int.MaxValue >> 1)) >> 1;
                var value = factory(nextOffsetMs);
                if (!found.Contains(value))
                {
                    results[i] = value;
                    found.Add(value);
                    i++;
                }
            }

            return (results, found[0], found[1], found[^1], found[^2]);
        }
    }
}

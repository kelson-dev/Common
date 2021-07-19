using Kelson.Common.DataStructures.Sets;
using System.Diagnostics;

namespace Kelson.Common.DataStructures.Tests
{
    public class ByteSet_Should
    {
        static readonly Random rng = new(917862);
        static List<byte> RandomList() => Enumerable.Range(0, rng.Next(0, 255)).Select(i => (byte)rng.Next(0, 255)).ToList();
        static (ByteSet test, SortedSet<byte> compare) RandomSets() 
        {
            var list = RandomList();
            return (new ByteSet(list), new SortedSet<byte>(list));   
        }

        [Fact]
        public void AddItems()
        {
            var set = new ByteSet().Add(0).Add(100).Add(200);
            set.Should().ContainInOrder(0, 100, 200);
        }

        [Fact]
        public void RemoveItems()
        {
            var set = new ByteSet(new byte[] { 0, 100, 200 });
            set = set.Remove(100);
            set.Should().ContainInOrder(0, 200);
        }

        [Fact]
        public void Except()
        {
            byte[] a = new byte[] { 0, 50, 100, 150, 200, 250 };
            var test1 = new ByteSet(a);
            var compare1 = new SortedSet<byte>(a);

            byte[] b = new byte[] { 50, 200, 250 };
            var test2 = new ByteSet(b);
            var compare2 = new SortedSet<byte>(b);

            compare1.ExceptWith(compare2);
            byte[] expected = compare1.ToArray();
            var result = test1.Except(test2);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ExceptRandom()
        {
            for (int i = 0; i < 50; i++)
            {
                var (test1, compare1) = RandomSets();
                var (test2, compare2) = RandomSets();

                var expectedSet = new SortedSet<byte>(compare1);
                expectedSet.ExceptWith(compare2);
                byte[] expected = expectedSet.ToArray();
                var result = test1.Except(test2);
                result.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public void SymmetricExcept()
        {
            byte[] a = new byte[] { 0, 50, 100, 150, 200, 250 };
            var test1 = new ByteSet(a);
            var compare1 = new SortedSet<byte>(a);

            byte[] b = new byte[] { 50, 200, 250 };
            var test2 = new ByteSet(b);
            var compare2 = new SortedSet<byte>(b);

            compare1.SymmetricExceptWith(compare2);
            byte[] expected = compare1.ToArray();
            var result = test1.SymmetricExcept(test2);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SymmetricExceptRandom()
        {
            for (int i = 0; i < 50; i++)
            {
                var (test1, compare1) = RandomSets();
                var (test2, compare2) = RandomSets();

                var expectedSet = new SortedSet<byte>(compare1);
                expectedSet.SymmetricExceptWith(compare2);
                byte[] expected = expectedSet.ToArray();
                var result = test1.SymmetricExcept(test2);
                if (!result.SequenceEqual(expected))
                    Debugger.Break();
                result.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public void Intersect()
        {
            for (int i = 0; i < 50; i++)
            {
                var (test1, compare1) = RandomSets();
                var (test2, compare2) = RandomSets();

                var expectedSet = new SortedSet<byte>(compare1);
                expectedSet.IntersectWith(compare2);
                byte[] expected = expectedSet.ToArray();
                var result = test1.Intersect(test2);
                result.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public void Union()
        {
            for (int i = 0; i < 50; i++)
            {
                var (test1, compare1) = RandomSets();
                var (test2, compare2) = RandomSets();

                var expectedSet = new SortedSet<byte>(compare1);
                expectedSet.UnionWith(compare2);
                byte[] expected = expectedSet.ToArray();
                var result = test1.Union(test2);
                result.Should().BeEquivalentTo(expected);
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(64, 1)]
        [InlineData(64, 64)]
        [InlineData(64, 63)]
        [InlineData(128, 1)]
        [InlineData(128, 64)]
        [InlineData(128, 127)]
        [InlineData(255, 0)]
        [InlineData(255, 1)]
        [InlineData(255, 64)]
        [InlineData(255, 128)]
        [InlineData(255, 170)]
        [InlineData(255, 200)]
        [InlineData(255, 255)]
        public void ShiftDownSingleItemSetToReflectSubtraction(byte value, byte shift)
        {
            ByteSet set = new(value);
            var shifted1 = set.ShiftDown(shift);
            var shifted2 = set >> shift;
            shifted1.Should().BeEquivalentTo(shifted2);
            shifted1.Should().BeEquivalentTo(new byte[] { (byte)(value - shift) });
        }

        [Fact]
        public void ShiftDown()
        {
            var set = new ByteSet(new byte[] { 0, 64, 200 });

            set >>= 1;

            set.Should().BeEquivalentTo(new byte[] { 63, 199 });

            set >>= 50;

            set.Should().BeEquivalentTo(new byte[] { 13, 149 });
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(63, 1)]
        [InlineData(127, 1)]
        [InlineData(191, 1)]
        [InlineData(192, 1)]
        [InlineData(254, 1)]
        public void ShiftUpSingleItemSetToReflectAddition(byte value, byte shift)
        {
            ByteSet set = new(value);
            var shifted1 = set.ShiftUp(shift);
            var shifted2 = set << shift;
            shifted1.Should().BeEquivalentTo(shifted2);
            shifted1.Should().BeEquivalentTo(new byte[] { (byte)(value + shift) });
        }

        [Fact]
        public void ShiftUp()
        {
            var set = new ByteSet(new byte[] { 0, 63, 200 });

            set <<= 1;

            set.Should().BeEquivalentTo(new byte[] { 1, 64, 201 });

            set <<= 50;

            set.Should().BeEquivalentTo(new byte[] { 51, 114, 251 });
        }
    }
}

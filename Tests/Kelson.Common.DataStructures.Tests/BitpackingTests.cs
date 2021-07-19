

namespace Kelson.Common.DataStructures.Tests
{
    public class BitpackingTests
    {
        [Fact]
        public unsafe void TestBitpackReaderRecievesBitsInCorrectOrderWithWidth7()
        {
            // The numbers 1, 2, 4, 8, 16, 32 packed with a width of 7
            Span<byte> packedData = stackalloc byte[] { 0b0_0000001, 0b00_000001, 0b000_00001, 0b0000_0001, 0b00000_001, 0b000000_01, };

            var reader = new BitpackReader(packedData);

            for (int i = 0; i < 6; i++)
            {
                reader.IsComplete(7).Should().BeFalse();
                int read = reader.ConsumeCode(7);
                int expected = 1 << i;
                read.Should().Be(expected);
            }

            reader.IsComplete(7).Should().BeTrue();
        }

        [Fact]
        public unsafe void TestBitpackReaderRecievesBitsInCorrectOrderWithWidth5()
        {
            Span<byte> packedData = stackalloc byte[] { 1, 1, 1, 1, 1, 1 };
            int[] output = new int[9];
            int[] expected = new int[] { 1, 8, 0, 2, 16, 0, 4, 0, 1 };

            var reader = new BitpackReader(packedData);
            for (int i = 0; i < output.Length; i++)
            {
                reader.IsComplete(5).Should().BeFalse();
                output[i] = reader.ConsumeCode(5);
            }
            reader.IsComplete(5).Should().BeTrue();
            output.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public unsafe void TestBitpackWriterClearsBuffer()
        {
            const int LENGTH = 621;
            byte* buffer = stackalloc byte[LENGTH];
            var random = new Random();
            var span = new Span<byte>(buffer, LENGTH);
            random.NextBytes(span);

            var writer = new BitpackWriter(buffer, LENGTH);

            for (int i = 0; i < LENGTH; i++)
                buffer[i].Should().Be(0);
        }

        [Fact]
        public void TestBitpackWriterPacksWithFixedWidthToExpected()
        {
            Span<byte> buffer = stackalloc byte[6];

            int[] input = new int[] { 1, 8, 0, 2, 16, 0, 4, 0, 1 };
            byte[] expected = new byte[] { 1, 1, 1, 1, 1, 1 };

            var writer = new BitpackWriter(buffer);

            foreach (int item in input)
            {
                writer.CanAppend(5).Should().BeTrue();
                writer.Append(item, 5);
            }
            writer.CanAppend(5).Should().BeFalse();

            buffer.ToArray().Should().BeEquivalentTo(expected);
        }
    }
}

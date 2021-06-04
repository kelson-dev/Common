using static Kelson.d3iflib.Compression.Lzw;
using System.Collections.Generic;
using FluentAssertions;
using System;
using Xunit;
using System.Linq;

namespace Kelson.d3iflib.Tests
{
    public class LzwCompressionTests
    {
        [Fact]
        public unsafe void TestDecompressPackedDataFromSampleGif()
        {
            Span<byte> packedData = stackalloc byte[] { 0b01000100, 0b00011110, 0b10000110, 0b01111010, 0b01010000, };

            byte[] expected = new byte[] { 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0 };

            var input = packedData.ToArray().ToList();

            var reader = new BitpackReader(packedData);


            throw new NotImplementedException();
        }

        [Fact]
        public unsafe void TestBitpackReaderRecievesBitsInCorrectOrderWithWidth7()
        {
            // The numbers 1, 2, 4, 8, 16, 32 packed with a width of 7
            Span<byte> packedData = stackalloc byte[] { 0b0_0000001, 0b00_000001, 0b000_00001, 0b0000_0001, 0b00000_001, 0b000000_01, };

            var reader = new BitpackReader(packedData);

            for (int i = 0; i < 6; i++)
            {
                reader.IsComplete(7).Should().BeFalse();
                var read = reader.ConsumeCode(7);
                var expected = 1 << i;
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

            foreach (var item in input)
            {
                writer.CanAppend(5).Should().BeTrue();
                writer.Append(item, 5);
            }
            writer.CanAppend(5).Should().BeFalse();
            
            buffer.ToArray().Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void TestGifCompressionOfSampleStreamProducesExpected()
        {
            Span<byte> example_image = stackalloc byte[] { 
                1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 
                1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 
                1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 
                1, 1, 1, 0, 0, 0, 0, 2, 2, 2, 
                1, 1, 1, 0, 0, 0, 0, 2, 2, 2,
                2, 2, 2, 0, 0, 0, 0, 1, 1, 1,
                2, 2, 2, 0, 0, 0, 0, 1, 1, 1,
                2, 2, 2, 2, 2, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 1, 1, 1, 1, 1,
            };

            Span<byte> compression_buffer = stackalloc byte[example_image.Length];
            var writer = new BitpackWriter(compression_buffer);

            GifCompress(4, example_image, writer);

            Span<byte> decompression_buffer = stackalloc byte[example_image.Length];
            var reader = new BitpackReader(compression_buffer);
            var codeTable = new LzwCodeTable(4);
            var length = GifDecompress(ref codeTable, reader, decompression_buffer);
            var original = decompression_buffer[..length];
            original.ToArray().Should().BeEquivalentTo(example_image.ToArray());
        }

        [Fact]
        public void TestGifCompressionOfSample2GifFirstFrame()
        {
            Span<byte> example_image = stackalloc byte[] { 
                0, 1, 1, 1, 
                1, 0, 1, 1,
                1, 1, 0, 1,
                1, 1, 1, 0,
            };
            byte initialCodeWidth = 2;

            Span<byte> compression_buffer = stackalloc byte[example_image.Length];
            var writer = new BitpackWriter(compression_buffer);
#if DEBUG
            List<(int, int)> written = new();
            writer.OnValueWrote += (i, w) => written.Add((i, w));
#endif

            var compressed = GifCompress(initialCodeWidth, example_image, writer);

            Span<byte> decompression_buffer = stackalloc byte[example_image.Length];
            var reader = new BitpackReader(compression_buffer);
            var codeTable = new LzwCodeTable(initialCodeWidth);
            var length = GifDecompress(ref codeTable, reader, decompression_buffer);
            var original = decompression_buffer[..length];
            original.ToArray().Should().BeEquivalentTo(example_image.ToArray());
        }

        [Fact]
        public void TestWriterReaderCompletesRoundTripWithSampleImageExpectedEncodedValues()
        {
            byte[] width = new byte[]{ 3, 3, 4, 4, 4, 4, 4,  4, 4, 4, 4 };
            int[] values = new int[] { 4, 0, 1, 7, 1, 6, 8, 10, 7, 0, 5 };

            Span<byte> buffer = stackalloc byte[values.Length];

            var writer = new BitpackWriter(buffer);
            for (int i = 0; i < values.Length; i++)
                writer.Append(values[i], width[i]);

            //buffer[..4].ToArray().Should().BeEquivalentTo(188, 67, 189, 2);

            var reader = new BitpackReader(writer.Written);
            for (int i = 0; i < values.Length; i++)
            {
                var read = reader.ConsumeCode(width[i]);
                var expected = values[i];
                read.Should().Be(expected);
            }
        }

        [Fact]
        public void TestWriterReaderCompletesRoundTripWithVariableSymbolWidth()
        {
            byte[] width = new byte[]{ 2, 2, 3, 3, 3,  4, 4, 4,  4,  6,  6,   7,   7,   7 };
            int[] values = new int[] { 1, 3, 7, 4, 0, 12, 3, 3, 11, 40, 33, 104, 105, 106 };

            Span<byte> buffer = stackalloc byte[values.Length];

            var writer = new BitpackWriter(buffer);

            for (int i = 0; i < values.Length; i++)
                writer.Append(values[i], width[i]);

            var reader = new BitpackReader(writer.Written);
            for (int i = 0; i < values.Length; i++)
            {
                var (read, expected) = (reader.ConsumeCode(width[i]), values[i]);
                read.Should().Be(expected);
            }
            reader.IsComplete(width[^1]).Should().BeTrue();
        }
    }
}

using System;
using System.Collections.Generic;
#if DEBUG
using System.Linq;
#endif

namespace Kelson.d3iflib.Compression
{
    public static class Lzw
    {
        public unsafe static int GifDecompress(ref LzwCodeTable codeTable, BitpackReader reader, Span<byte> outputBuffer,
            bool omitInitialClearCode = false)
        {
            static void writeOutput(byte[] sequence, ref int index, Span<byte> output)
            {
                for (int i = 0; i < sequence.Length; i++)
                    output[index + i] = sequence[i];
                index += sequence.Length;
            }

            var initialWidth = codeTable.SymbolWidth - 1;

            var first = reader.ConsumeCode(codeTable.SymbolWidth);

            if (!omitInitialClearCode && first != codeTable.ClearCode)
                throw new GifParsingException("Expected clear code at the start of compressed image data");

            int output_index = 0;

            var code = omitInitialClearCode ? first : reader.ConsumeCode(codeTable.SymbolWidth);

            writeOutput(codeTable[code], ref output_index, outputBuffer);

            while (!reader.IsComplete(codeTable.SymbolWidth))
            {
#if DEBUG
                var so_far = outputBuffer[..output_index].ToArray();
#endif
                var previous_sequence = codeTable[code];
                code = reader.ConsumeCode(codeTable.SymbolWidth);
                if (code == codeTable.EndOfInformationCode)
                    return output_index;
                else if (code == codeTable.ClearCode)
                    codeTable = new LzwCodeTable(codeTable.InitialSymbolWidth);
                else if (codeTable.TryGet(code, out byte[] next_code_sequence))
                {
                    writeOutput(next_code_sequence, ref output_index, outputBuffer);
                    var k = next_code_sequence[0];
                    codeTable.Append(previous_sequence.Append(k));
                }
                else
                {
                    var k = previous_sequence[0];
                    var new_entry = previous_sequence.Append(k).ToArray();
                    writeOutput(new_entry, ref output_index, outputBuffer);
                    codeTable.Append(new_entry);
                }
            }

            return output_index;
        }


        /// <summary>
        /// Compresses a span of data using the GIF variation of LZW compression
        /// </summary>
        /// <param name="colorTableSize"></param>
        /// <param name="data">Uncompressed data to encode</param>
        /// <param name="writer">Target stream for outputing variable width symbols</param>
        public unsafe static ReadOnlySpan<byte> GifCompress(byte initialCodeWidth, ReadOnlySpan<byte> data, BitpackWriter writer,
            bool omit_cc = false,
            bool omit_eoi = false)
        {
            byte codeWidth = initialCodeWidth;

            int clear_code = (1 << initialCodeWidth);
            int eoi_code = clear_code + 1;
            int next_entry = clear_code + 2;

            // codeWidth must be large enough to encode clear_code
            codeWidth++;

            Span<byte> index_buffer = stackalloc byte[1023];
            byte* single_fetch = stackalloc byte[1];


            Dictionary<int, byte[]> codeTable = new();

            bool sequenceExists(ReadOnlySpan<byte> sequence, out int index)
            {
                foreach (var (key, value) in codeTable)
                {
                    if (value.Length == sequence.Length)
                    {
                        bool matches = true;
                        for (int i = 0; i < value.Length; i++)
                        {
                            if (value[i] != sequence[i])
                            {
                                matches = false;
                                break;
                            }
                        }
                        if (matches)
                        {
                            index = key;
                            return true;
                        }
                    }
                }
                index = 0;
                return false;
            }

            void recordSequence(ReadOnlySpan<byte> sequence)
            {
                if (next_entry >= (1 << codeWidth))
                    codeWidth++;
                codeTable[next_entry++] = sequence.ToArray();
            }

            if (!omit_cc)
                writer.Append(clear_code, codeWidth);

            index_buffer[0] = data[0];
            var working_buffer = index_buffer[..1];
            for (int i = 1; i < data.Length; i++)
            {
                var k = data[i];
                index_buffer[working_buffer.Length] = k;
                var working_buffer_plus_k = index_buffer[..(working_buffer.Length + 1)];

                if (sequenceExists(working_buffer_plus_k, out int index))
                {
                    working_buffer = working_buffer_plus_k;
                }
                else
                {
                    recordSequence(working_buffer_plus_k);
                    if (sequenceExists(working_buffer, out int index_of_working_buffer))
                        writer.Append(index_of_working_buffer, codeWidth);
                    else
                        for (int s = 0; s < working_buffer.Length; s++)
                            writer.Append(working_buffer[s], codeWidth);
                    index_buffer[0] = k;
                    working_buffer = index_buffer[..1];
                }
            }

            if (sequenceExists(working_buffer, out int last_index))
                writer.Append(last_index, codeWidth);
            else
                for (int s = 0; s < working_buffer.Length; s++)
                    writer.Append(working_buffer[s], codeWidth);

            // end of image
            if (!omit_eoi)
                writer.Append(eoi_code, codeWidth);

            return writer.Written;
        }

        public unsafe ref struct BitpackReader
        {
            private const int BYTE_LENGTH = 8;

            private readonly ReadOnlySpan<byte> data;

            private int bit;
            public int Bit => bit;

#if DEBUG
            public event Action<int> OnRead;
#endif

            public BitpackReader(byte* data, byte length, int offset = 0)
            {
                this.data = new ReadOnlySpan<byte>(data, length);
                this.bit = offset;
#if DEBUG
                this.OnRead = i => { };
#endif
            }

            public BitpackReader(ReadOnlySpan<byte> data, int offset = 0)
            {
                this.data = data;
                this.bit = offset;
#if DEBUG
                this.OnRead = i => { };
#endif
            }

            public bool IsComplete(byte codeWidth) => codeWidth > 12 || bit + codeWidth > data.Length * 8;

            public int ConsumeCode(byte codeWidth)
            {
                var (bitIndex, byteIndex) = (bit % BYTE_LENGTH, bit / BYTE_LENGTH);
                int mask_current = (((1 << codeWidth) - 1) << bitIndex) & 0xFF;
                int current = data[byteIndex];
#if DEBUG
                var debug_current = Convert.ToString(data[byteIndex], 2);
                var debug_mask = Convert.ToString(mask_current, 2);
                var debug_selected = Convert.ToString(current & mask_current, 2);
#endif

                int value = (current & mask_current) >> bitIndex;
                int take_from_next = codeWidth + bitIndex - 8;
                if (take_from_next > 0 && byteIndex + 1 < data.Length)
                {
#if DEBUG
                    var debug_next = Convert.ToString(data[byteIndex + 1], 2);
#endif
                    int mask_next = ~(~0 >> take_from_next << take_from_next);
                    byte value_next = (byte)(data[byteIndex + 1] & mask_next);
                    value |= value_next << (codeWidth - take_from_next);
                }

                bit += codeWidth;
#if DEBUG
                OnRead(value);
#endif
                return value;
            }
        }

        public unsafe ref struct BitpackWriter
        {
            private const int BYTE_LENGTH = 8;

            private readonly Span<byte> data;

            private int _bitIndex;
            private int bitIndex
            {
                get => _bitIndex;
                set => _bitIndex = value;
            }

            public ReadOnlySpan<byte> Written => data[..(bitIndex / BYTE_LENGTH + 1)];

#if DEBUG
            public event Action<int, int> OnValueWrote;
#endif

            public BitpackWriter(byte* data, ushort length)
            {
                this.data = new Span<byte>(data, length);
                this.data.Clear(); // Cannot assume supplied buffer is 0 indexed, but append benefits from this assumption
                this._bitIndex = 0;
#if DEBUG
                OnValueWrote = (i, w) => { };
#endif
            }

            public BitpackWriter(Span<byte> data)
            {
                this.data = data;
                this.data.Clear();
                this._bitIndex = 0;
#if DEBUG
                OnValueWrote = (i, w) => { };
#endif
            }

            public bool CanAppend(byte codeWidth) => codeWidth <= 12 && bitIndex + codeWidth < data.Length * 8;

            public void Append(int value, byte codeWidth)
            {
#if DEBUG
                OnValueWrote(value, codeWidth);
#endif
                var (bit, index) = (bitIndex % BYTE_LENGTH, bitIndex / BYTE_LENGTH);

                int symbol_mask = (1 << codeWidth) - 1;

                int mask_current = ((1 << (BYTE_LENGTH - bit)) - 1) & symbol_mask;
                int mask_next = mask_current ^ symbol_mask;

                int first_value = value & mask_current;
                first_value <<= bit;

                data[index] |= (byte)first_value;

                if (mask_next != 0)
                {
                    int value_bit = BYTE_LENGTH - bit;
                    int second_value = value & mask_next;
                    second_value >>= value_bit;
                    data[index + 1] = (byte)second_value;
                }

                bitIndex += codeWidth;
            }
        }

        public readonly ref struct GifDecompressionResult
        {
            public readonly int Length;
            public readonly int Bit;

            public GifDecompressionResult(int length, int readerBit)
            {
                Length = length;
                Bit = readerBit;
            }

            public void Deconstruct(out int length, out int readerBit) => (length, readerBit) = (Length, Bit);
        }

        public class LzwCodeTable
        {
            public byte SymbolWidth { get; private set; }

            public readonly byte InitialSymbolWidth;
            private readonly int initialTableSize;
            public readonly int ClearCode;
            public readonly int EndOfInformationCode;

            private readonly Dictionary<int, byte[]> _codeTable = new();

            public LzwCodeTable(byte initialSymbolWidth)
            {
                InitialSymbolWidth = initialSymbolWidth;
                SymbolWidth = initialSymbolWidth;
                initialTableSize = 1 << initialSymbolWidth;
                ClearCode = initialTableSize;
                EndOfInformationCode = initialTableSize + 1;

                for (int i = 0; i <= EndOfInformationCode; i++)
                    _codeTable[i] = i > 0xFF
                        ? Array.Empty<byte>()
                        : new byte[] { (byte)i };

                SymbolWidth++;
            }

            public bool TryGet(int key, out byte[] data) => _codeTable.TryGetValue(key, out data);

            public byte[] this[int key] => _codeTable[key];

            public int Count => _codeTable.Count;

            public int MinWidth => (int)Math.Log2(_codeTable.Count);

            public void Append(IEnumerable<byte> sequence)
            {
                _codeTable[Count] = sequence.ToArray();
                if (Count >= 1 << SymbolWidth)
                    SymbolWidth++;
            }
        }
    }

    public class GifParsingException : Exception
    {
        public GifParsingException(string message) : base(message)
        { 
        }
    }
}

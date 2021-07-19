using System;

namespace Kelson.Common.DataStructures.Encodings
{
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

        public bool IsComplete(byte codeWidth) => bit + codeWidth > data.Length * 8;

        public int ConsumeCode(byte codeWidth)
        {
            var (bitIndex, byteIndex) = (bit % BYTE_LENGTH, bit / BYTE_LENGTH);
            int mask_current = (((1 << codeWidth) - 1) << bitIndex) & 0xFF;
            int current = data[byteIndex];
#if DEBUG
            string? debug_current = Convert.ToString(data[byteIndex], 2);
            string? debug_mask = Convert.ToString(mask_current, 2);
            string? debug_selected = Convert.ToString(current & mask_current, 2);
#endif

            int value = (current & mask_current) >> bitIndex;
            int take_from_next = codeWidth + bitIndex - 8;
            if (take_from_next > 0 && byteIndex + 1 < data.Length)
            {
#if DEBUG
                string? debug_next = Convert.ToString(data[byteIndex + 1], 2);
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

        public ReadOnlySpan<byte> Written => data[..((bitIndex / BYTE_LENGTH) + 1)];

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
}

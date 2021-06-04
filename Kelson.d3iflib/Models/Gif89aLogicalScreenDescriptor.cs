using System.Runtime.InteropServices;

namespace Kelson.d3iflib.Models
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Gif89aLogicalScreenDescriptor
    {
        const int ColorTableSizeMask = 0b0111;
        const int ColorTableSortFlagMask = 0b1000;
        const int ColorBitDepthMask = 0b0111_0000;
        const int ColorTableFlagMask = 0b1000_0000;

        [FieldOffset(0)]
        private byte packed;

        public ulong GlobalTableSize
        {
            get => 1UL << ((packed & ColorTableSizeMask) + 1);
            set
            {
                byte size = 0;
                while (value > 0 && ((value >>= 1) & 1UL) != 1UL)
                    size++;
                packed = (byte)((packed & ~ColorTableSizeMask) | (size & ColorTableSizeMask));
            }
        }

        public bool IsColorTableSorted
        {
            get => (packed & ColorTableSortFlagMask) == ColorTableSortFlagMask;
            set => packed = value
                ? (byte)((packed & ~ColorTableSortFlagMask) | ColorTableSortFlagMask)
                : (byte)(packed & ~ColorTableSortFlagMask);
        }

        public int BitDepth
        {
            get => ((packed & ColorBitDepthMask) >> 4) + 1;
            set
            {
                value--;
                packed = (byte)((packed & ~ColorBitDepthMask) | ((value & 0b0111) << 4));
            }
        }

        public bool HasGlobalColorTable
        {
            get => (packed & ColorTableFlagMask) == ColorTableFlagMask;
            set => packed = value
                ? (byte)((packed & ~ColorTableFlagMask) | ColorTableFlagMask)
                : (byte)(packed & ~ColorTableFlagMask);
        }
    }
}

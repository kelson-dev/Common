using System.Runtime.InteropServices;

namespace Kelson.d3iflib.Models
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Gif89aImageDescriptorData
    {
        const int LocalColorTableFlagMask = 0b0001;
        const int InterlaceFlagMask = 0b0010;
        const int SortFlagMask = 0b0100;
        const int ReservedFlagMask = 0b0001_1000;
        const int ColorTableEntrySizeMask = 0b1110_0000;

        [FieldOffset(0)]
        private byte packed;

        public bool HasLocalColorTable
        {
            get => (packed & LocalColorTableFlagMask) == 1;
            set => packed = value
                ? (byte)((packed & ~LocalColorTableFlagMask) | LocalColorTableFlagMask)
                : (byte)(packed & ~LocalColorTableFlagMask);
        }

        public bool IsInterlaced
        {
            get => (packed & InterlaceFlagMask) == 1;
            set => packed = value
                ? (byte)((packed & ~InterlaceFlagMask) | InterlaceFlagMask)
                : (byte)(packed & ~InterlaceFlagMask);
        }

        public bool IsColorTablePrioritySorted
        {
            get => (packed & SortFlagMask) == 1;
            set => packed = value
                ? (byte)((packed & ~SortFlagMask) | SortFlagMask)
                : (byte)(packed & ~SortFlagMask);
        }

        public int BitDepth
        {
            get => (packed & ColorTableEntrySizeMask);
            set => packed = (byte)((packed & ~ColorTableEntrySizeMask) | ((value & 0b0111) << 5));
        }
    }
}

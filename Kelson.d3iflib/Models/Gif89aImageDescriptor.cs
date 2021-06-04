using System.Runtime.InteropServices;

namespace Kelson.d3iflib.Models
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Gif89aImageDescriptor
    {
        [FieldOffset(0)]
        public byte Seperator;
        [FieldOffset(1)]
        public ushort Left;
        [FieldOffset(3)]
        public ushort Top;
        [FieldOffset(5)]
        public ushort Width;
        [FieldOffset(7)]
        public ushort Height;
        [FieldOffset(9)]
        public Gif89aImageDescriptorData Info;
    }
}

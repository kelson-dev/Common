using System.Runtime.InteropServices;

namespace Kelson.d3iflib.Models
{
    public enum GifVersion
    {

    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Gif89aHeader
    {
        [FieldOffset(0)]
        private readonly byte signature0;
        [FieldOffset(1)]
        private readonly byte signature1;
        [FieldOffset(2)]
        private readonly byte signature2;
        public string Signature => $"{(char)signature0}{(char)signature1}{(char)signature2}";

        [FieldOffset(3)]
        private readonly byte version0;
        [FieldOffset(4)]
        private readonly byte version1;
        [FieldOffset(5)]
        private readonly byte version2;

        public string Version => $"{(char)version0}{(char)version1}{(char)version2}";

        [FieldOffset(6)]
        private readonly ushort width;
        public int Width { get => width; /*set => width = (ushort)value; */}
        

        [FieldOffset(8)]
        private readonly ushort height;
        public int Height { get => height; /*set => height = (ushort)value; */}

        [FieldOffset(10)]
        public readonly Gif89aLogicalScreenDescriptor ScreenDescriptor;

        [FieldOffset(11)]
        private readonly byte backgroundColorIndex;
        public int BackgroundColorIndex { get => backgroundColorIndex; /*set => backgroundColorIndex = (byte)value; */}

        [FieldOffset(12)]
        private readonly byte aspectRatio;
        public byte AspectRatio => aspectRatio;

        public double PixelAspectRatio
        {
            get => (aspectRatio + 15) / 64;
            //set => aspectRatio = (byte)((value * 64) - 15);
        }
    }
}

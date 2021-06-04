using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Kelson.d3iflib.Models
{
    public partial class Gif89a
    {
        public Gif89aHeader Header { get; private set; }
        public IColorTable? GlobalColorTable { get; private set; }
        public readonly List<string> Comments = new();
        public readonly List<Gif89aGraphicsControlExtensionBlock> GraphicsControlHeaders = new();
        public readonly List<Gif89aApplicationExtensionBlockHeader> ApplicationExtensionHeaders = new();
        public readonly List<byte[]> ApplicationData = new();
        public readonly List<Gif89aPlainTextExtensionBlockHeader> PlainTextExtensionHeaders = new();
        public readonly List<string> PlainTextBlocks = new();
        public readonly List<Gif89aImage> Images = new();
    }

    public readonly struct Gif89aColorRGB
    {
        public readonly byte Red;
        public readonly byte Green;
        public readonly byte Blue;

        public override int GetHashCode() => Red << 16 | Green << 8 | Blue;

        public static bool operator ==(Gif89aColorRGB a, Gif89aColorRGB b) => a.GetHashCode() == b.GetHashCode();

        public static bool operator !=(Gif89aColorRGB a, Gif89aColorRGB b) => a.GetHashCode() != b.GetHashCode();

        public override bool Equals(object? obj) => obj is Gif89aColorRGB color && color == this;

        public override string ToString() => $"[{Red,3}, {Green,3}, {Blue,3}]";
    }

    public enum BlockLabel : byte
    {
        Image = 0x2C,
        Extension = 0x21,
        EndOfFile = 0x3B
    }

    public enum ExtensionLabel : byte
    {
        Comment = 0xFE,
        GraphicsControl = 0xF9,
        PlainText = 0x01,
        Application = 0xFF
    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Gif89aCommentExtensionBlockHeader
    {
        [FieldOffset(0)]
        private readonly BlockLabel introducer;

        [FieldOffset(1)]
        private readonly ExtensionLabel label;
    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Gif89aGraphicsControlExtensionBlock
    {
        [FieldOffset(0)]
        private readonly BlockLabel introducer;

        [FieldOffset(1)]
        private readonly ExtensionLabel label;

        [FieldOffset(2)]
        private readonly byte blockSize;

        [FieldOffset(3)]
        private readonly byte packed;

        [FieldOffset(4)]
        private readonly ushort delayTime;

        [FieldOffset(6)]
        private readonly byte colorIndex;

        [FieldOffset(7)]
        private readonly byte terminator;
    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Gif89aPlainTextExtensionBlockHeader
    {
        [FieldOffset(0)]
        private readonly BlockLabel introducer;

        [FieldOffset(1)]
        private readonly ExtensionLabel label;

        [FieldOffset(2)]
        private readonly byte blockSize;

        [FieldOffset(3)]
        private readonly ushort textGridLeft;

        [FieldOffset(5)]
        private readonly ushort textGridTop;

        [FieldOffset(7)]
        private readonly ushort textGridWidth;

        [FieldOffset(9)]
        private readonly ushort textGridHeight;

        [FieldOffset(10)]
        private readonly byte cellWidth;

        [FieldOffset(11)]
        private readonly byte cellHeight;

        [FieldOffset(12)]
        private readonly byte textFgColorIndex;

        [FieldOffset(13)]
        private readonly byte textBgColorIndex;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Gif89aApplicationExtensionBlockHeader
    {
        [FieldOffset(0)]
        private readonly BlockLabel introducer;

        [FieldOffset(1)]
        private readonly ExtensionLabel label;

        [FieldOffset(2)]
        private readonly byte blockSize;

        [FieldOffset(3)]
        private fixed char identifier[8];

        [FieldOffset(19)]
        private fixed byte authentCode[3];
    }

    public interface IColorTable
    {
        Gif89aColorRGB this[byte index] { get; }
        byte this[Gif89aColorRGB color] { get; }
        int Length { get; }
    }

    public class BasicColorTable : IColorTable
    {
        private readonly Gif89aColorRGB[] colors;

        public int Length => colors.Length;

        public byte this[Gif89aColorRGB color]
        {
            get
            {
                for (byte i = 0; i < Length; i++)
                    if (colors[i] == color)
                        return i;
                return 0;  
            }
        }

        public Gif89aColorRGB this[byte index] => colors[index];

        public unsafe BasicColorTable(ReadOnlySpan<Gif89aColorRGB> data)
        {
            colors = new Gif89aColorRGB[data.Length];
            data.CopyTo(colors);
        }
    }

    public class Gif89aImage
    {
        public readonly Gif89aImageDescriptor Descriptor;
        public readonly IColorTable Colors;
        public readonly byte[] Indecies;

        public Gif89aImage(Gif89aImageDescriptor descriptor, IColorTable table)
        {
            Descriptor = descriptor;
            Colors = table;
            Indecies = new byte[descriptor.Width * descriptor.Height];
        }

        public Gif89aColorRGB this[int x, int y]
        {
            get
            {
                var index = (Descriptor.Width * y) + x;
                return Colors[Indecies[index]];
            }
        }

        public byte this[int index]
        {
            set
            {
                Indecies[index] = value;
            }
        }

        public override string ToString()
        {
            var debugText = "";
            for (int i = 0; i < Indecies.Length; i++)
            {
                debugText += $" {Indecies[i]}";
                if (i % Descriptor.Width == 0)
                    debugText += Environment.NewLine;
            }
            return debugText;
        }
    }

    public interface IImageData
    {
        public ushort Width { get; }
        public ushort Height { get; }
        public IColorTable ColorTable { get; }

        public Gif89aColorRGB this[int row, int column] { get; set; }
    }


    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Gif89aColorTable2 : IColorTable
    {
        [FieldOffset(0)]
        fixed byte Colors[2 * 3];

        public int Length => 2;

        public byte this[Gif89aColorRGB color] 
        { 
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    for (byte i = 0; i < Length; i++)
                        if (colors[i] == color)
                            return i;
                    return 0;
                }
            }
        }

        public Gif89aColorRGB this[byte index]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    return *(colors + index * 3);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Gif89aColorTable4 : IColorTable
    {
        [FieldOffset(0)]
        fixed byte Colors[4 * 3];

        public int Length => 4;

        public byte this[Gif89aColorRGB color]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    for (byte i = 0; i < Length; i++)
                        if (colors[i] == color)
                            return i;
                    return 0;
                }
            }
        }

        public Gif89aColorRGB this[byte index]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    return *(colors + index * 3);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Gif89aColorTable8 : IColorTable
    {
        [FieldOffset(0)]
        fixed byte Colors[8 * 3];

        public int Length => 8;

        public byte this[Gif89aColorRGB color]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    for (byte i = 0; i < Length; i++)
                        if (colors[i] == color)
                            return i;
                    return 0;
                }
            }
        }

        public Gif89aColorRGB this[byte index]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    return *(colors + index * 3);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Gif89aColorTable16 : IColorTable
    {
        [FieldOffset(0)]
        fixed byte Colors[16 * 3];

        public int Length => 16;

        public byte this[Gif89aColorRGB color]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    for (byte i = 0; i < Length; i++)
                        if (colors[i] == color)
                            return i;
                    return 0;
                }
            }
        }

        public Gif89aColorRGB this[byte index]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    return *(colors + index * 3);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Gif89aColorTable32 : IColorTable
    {
        [FieldOffset(0)]
        fixed byte Colors[32 * 3];

        public int Length => 32;

        public byte this[Gif89aColorRGB color]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    for (byte i = 0; i < Length; i++)
                        if (colors[i] == color)
                            return i;
                    return 0;
                }
            }
        }

        public Gif89aColorRGB this[byte index]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    return *(colors + index * 3);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Gif89aColorTable64 : IColorTable
    {
        [FieldOffset(0)]
        fixed byte Colors[64 * 3];

        public int Length => 64;

        public byte this[Gif89aColorRGB color]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    for (byte i = 0; i < Length; i++)
                        if (colors[i] == color)
                            return i;
                    return 0;
                }
            }
        }

        public Gif89aColorRGB this[byte index]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    return *(colors + index * 3);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Gif89aColorTable128 : IColorTable
    {
        [FieldOffset(0)]
        fixed byte Colors[128 * 3];

        public int Length => 128;

        public byte this[Gif89aColorRGB color]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    for (byte i = 0; i < Length; i++)
                        if (colors[i] == color)
                            return i;
                    return 0;
                }
            }
        }

        public Gif89aColorRGB this[byte index]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    return *(colors + index * 3);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Gif89aColorTable256 : IColorTable
    {
        [FieldOffset(0)]
        fixed byte Colors[256 * 3];

        public int Length => 256;

        public byte this[Gif89aColorRGB color]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    for (byte i = 0; i < Length; i++)
                        if (colors[i] == color)
                            return i;
                    return 0;
                }
            }
        }

        public Gif89aColorRGB this[byte index]
        {
            get
            {
                fixed (void* data = &this)
                {
                    var colors = (Gif89aColorRGB*)data;
                    return *(colors + index * 3);
                }
            }
        }
    }
}

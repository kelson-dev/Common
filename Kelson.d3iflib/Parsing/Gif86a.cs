using Kelson.d3iflib.Compression;
using System;
using System.Text;
using static Kelson.d3iflib.Compression.Lzw;

namespace Kelson.d3iflib.Models
{
    public partial class Gif89a
    {
        public static bool TryParse(byte[] data, out Gif89a result)
        {
            result = new Gif89a();

            unsafe
            {
                fixed (byte* bytes = &data[0])
                {
                    Gif89aHeader header = *(Gif89aHeader*)bytes;
                    result.Header = header;
                    int headerSize = sizeof(Gif89aHeader);
                    byte* colorTable = bytes + 13;
                    if (header.ScreenDescriptor.HasGlobalColorTable)
                        result.GlobalColorTable = toColorTable(colorTable, header.ScreenDescriptor.GlobalTableSize);
                    byte* cursor = colorTable + (header.ScreenDescriptor.GlobalTableSize * 3);

                    int ScanToTerminator(byte terminator = 0)
                    {
                        int index = 0;
                        while (cursor[index] != terminator)
                            index++;
                        return index;
                    }

                    Span<byte> lzwPixelIndexBuffer = stackalloc byte[4096];

                    while (cursor < bytes + data.Length)
                    {
#if DEBUG
                        var debug_trail = new ReadOnlySpan<byte>(cursor - 12, 12);
                        var debug_next = new ReadOnlySpan<byte>(cursor, 20);
#endif
                        switch ((BlockLabel)(cursor[0]))
                        {
                            case BlockLabel.Extension:
                                switch ((ExtensionLabel)(cursor[1]))
                                {
                                    case ExtensionLabel.Comment:
                                        cursor += sizeof(Gif89aCommentExtensionBlockHeader);
                                        var commentEndIndex = ScanToTerminator();
                                        byte[] commentData = new byte[commentEndIndex];
                                        new ReadOnlySpan<byte>(cursor, commentEndIndex).CopyTo(commentData);
                                        result.Comments.Add(Encoding.GetEncoding(0).GetString(new ReadOnlySpan<byte>(cursor, commentEndIndex)));
                                        cursor += commentEndIndex + 1;
                                        break;
                                    case ExtensionLabel.Application:
                                        var appHeader = *(Gif89aApplicationExtensionBlockHeader*)cursor;
                                        result.ApplicationExtensionHeaders.Add(appHeader);
                                        cursor += sizeof(Gif89aApplicationExtensionBlockHeader);
                                        var appDataEndIndex = ScanToTerminator();
                                        byte[] appData = new byte[appDataEndIndex];
                                        new ReadOnlySpan<byte>(cursor, appDataEndIndex).CopyTo(appData);
                                        result.ApplicationData.Add(appData);
                                        cursor += appDataEndIndex + 1;
                                        break;
                                    case ExtensionLabel.GraphicsControl:
                                        var graphicsControl = *(Gif89aGraphicsControlExtensionBlock*)cursor;
                                        result.GraphicsControlHeaders.Add(graphicsControl);
                                        cursor += sizeof(Gif89aGraphicsControlExtensionBlock);
                                        break;
                                    case ExtensionLabel.PlainText:
                                        break;
                                }
                                break;

                            case BlockLabel.Image:
#if DEBUG
                                var debug_cursor = new ReadOnlySpan<byte>(cursor, 36);
#endif
                                var descriptor = *(Gif89aImageDescriptor*)cursor;
                                cursor += sizeof(Gif89aImageDescriptor);
                                var table = descriptor.Info.HasLocalColorTable
                                    ? toColorTable(cursor, result.Header.ScreenDescriptor.GlobalTableSize)
                                    : result.GlobalColorTable!;
                                cursor += descriptor.Info.HasLocalColorTable ? (result.Header.ScreenDescriptor.GlobalTableSize * 3) : 0;
                                var image = new Gif89aImage(descriptor, table);
                                byte codeWidth = cursor[0];
                                cursor++;

                                int foundPixels = 0;
                                byte blockSize = cursor[0];
                                cursor++;

                                var codeTable = new LzwCodeTable(codeWidth);
                                bool omitClearCode = false;
                                while (foundPixels < image.Indecies.Length && blockSize != 0)
                                {
                                    var reader = new BitpackReader(cursor, blockSize);

                                    var length = GifDecompress(ref codeTable, reader, lzwPixelIndexBuffer, omitClearCode);
                                    var decoded = lzwPixelIndexBuffer[..length];
                                    for (int i = 0; i < decoded.Length && i < image.Indecies.Length; i++)
                                        image[foundPixels + i] = decoded[i];
                                    foundPixels += decoded.Length;
                                    cursor += blockSize;
                                    blockSize = cursor[0];
                                    omitClearCode = true;
                                    cursor++;
                                }
                                result.Images.Add(image);
                                break;
                            case BlockLabel.EndOfFile:
                                return true;
                            default:
                                cursor++;
                                break;
                        }
                    }
                }
                return false;
            }
        }

        public unsafe static IColorTable toColorTable(byte* colorTable, ulong size) => new BasicColorTable(new ReadOnlySpan<Gif89aColorRGB>(colorTable, (int)size));
    }
}

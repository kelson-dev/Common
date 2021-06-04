using System;
using static System.IO.File;
using static System.Console;
using Kelson.d3iflib.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

const string SAMPLE = "Sample.gif";

WriteLine("Loading Sample");

byte[] data = ReadAllBytes(SAMPLE);
var actual = (Bitmap)Image.FromFile(SAMPLE);

if (Directory.Exists("DebugOutput"))
    Directory.Delete("DebugOutput", true);
Directory.CreateDirectory("DebugOutput");

if (Gif89a.TryParse(data, out Gif89a gif))
{
    var frames = actual.GetFrameCount(FrameDimension.Time);
    for (int frame = 0; frame < frames; frame++)
    {
        WriteLine($"Frame: {frame}");
        actual.SelectActiveFrame(FrameDimension.Time, frame);
        var mine = gif.Images[frame];

        for (int i = 0; i < mine.Descriptor.Width * mine.Descriptor.Height; i++)
        {
            var (x, y) = (i % actual.Width, i / actual.Width);
            var actualPixel = actual.GetPixel(x, y);
            var parsedPixel = mine[x, y];
            ForegroundColor = SameColor(actualPixel, parsedPixel)
                ? ConsoleColor.Green
                : ConsoleColor.Red;
            WriteLine($"({x,3}, {y,3}) Actual: [{actualPixel.R,3}, {actualPixel.G,3}, {actualPixel.B,3}] - Parsed: {parsedPixel}");
            ForegroundColor = ConsoleColor.White;
        }
        WriteLine();
    }
}

WriteLine("Done");

void SaveMine(int frame, Gif89aImage image)
{
    var (width, height) = (image.Descriptor.Width, image.Descriptor.Height);
    Bitmap output = new Bitmap(width, height);
    for (int i = 0; i < width * height; i++)
    {
        var (x, y) = (i % width, i / width);
        var pixel = image[x, y];
        output.SetPixel(x, y, Color.FromArgb(pixel.Red, pixel.Green, pixel.Blue));
    }
    output.Save($"DebugOutput/Parsed{frame}.png", ImageFormat.Png);
}

void SaveActual(int frame, Bitmap actual)
{
    var (width, height) = (actual.Width, actual.Height);
    Bitmap output = new Bitmap(width, height);
    for (int i = 0; i < width * height; i++)
    {
        var (x, y) = (i % width, i / width);
        output.SetPixel(x, y, actual.GetPixel(x, y));
    }
    output.Save($"DebugOutput/Actual{frame}.png", ImageFormat.Png);
}

bool SameColor(Color actual, Gif89aColorRGB found) => actual.R == found.Red && actual.G == found.Green && actual.B == found.Blue;

//string p3(byte v) => v.ToString().PadLeft(3);
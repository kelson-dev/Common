using System;
using System.Collections;
using System.Collections.Generic;

namespace Kelson.d3iflib.Models
{
    public class Gif89aImageFrame : IEnumerable<Gif89aColorRGB>
    {
        public readonly Gif89aImageDescriptor LocalImageDescriptor = default;
        public IColorTable ColorTable { get; internal set; }
        public IImageData ImageData { get; internal set; }

        public Gif89aImageFrame(Gif89aImageDescriptor localImageDescriptor, IColorTable colors, IImageData data = null) =>
            (LocalImageDescriptor, ColorTable, ImageData) = (localImageDescriptor, colors, data);

        public IEnumerator<Gif89aColorRGB> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}

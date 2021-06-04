using System;

namespace Kelson.d3iflib.Models
{
    public struct ImageData : IImageData
    {
        private readonly byte[] Indecies;
        private readonly ushort width;
        private readonly ushort height;
        private readonly IColorTable table;

        
        public Gif89aColorRGB this[int row, int column]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IColorTable ColorTable => table;
        public ushort Width => width;
        public ushort Height => height;
    }
}

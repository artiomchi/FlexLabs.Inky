using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FlexLabs.Inky.Preview
{
    class DrawingInkyDriver : IInkyDriver
    {
        private readonly byte[,,] _buffer;
        private readonly InkyDisplayColour _displayColour;
        private readonly WriteableBitmap _bitmap;

        public DrawingInkyDriver(int width, int height, InkyDisplayColour colour, WriteableBitmap bitmap)
        {
            Width = width;
            Height = height;
            _displayColour = colour;
            _bitmap = bitmap;
            _buffer = new byte[height, width, 4];
        }

        public InkyPixelColour BorderColour { get; set; }
        public InkyDisplayColour DisplayColour { get; set; }
        public bool FlipHorizontally { get; set; }
        public bool FlipVertically { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        private byte[] GetColor(InkyPixelColour colour)
        {
            return (colour, _displayColour) switch
            {
                (InkyPixelColour.Black, _) => new byte[] { 0, 0, 0, 255 },
                (InkyPixelColour.White, _) => new byte[] { 255, 255, 255, 255 },
                (InkyPixelColour.Red, InkyDisplayColour.Black) => new byte[] { 0, 0, 0, 255 },
                (InkyPixelColour.Red, InkyDisplayColour.Yellow) => new byte[] { 255, 215, 0, 255 },
                (InkyPixelColour.Red, _) => new byte[] { 139, 0, 0, 255 },
                (_, _) => throw new InvalidOperationException(),
            };
        }

        public Task Init()
        {
            for (var h = 0; h < Height; h++)
                for (var w = 0; w < Width; w++)
                    for (var c = 0; c < 4; c++)
                        _buffer[h, w, c] = 255;
            return Task.CompletedTask;
        }

        public void Clear(InkyPixelColour colour = InkyPixelColour.White)
        {
            var colourBytes = GetColor(colour);
            for (var h = 0; h < Height; h++)
                for (var w = 0; w < Width; w++)
                    for (var c = 0; c < colourBytes.Length; c++)
                        _buffer[h, w, c] = colourBytes[c];
        }

        public Task RenderBuffer()
        {
            // Copy the data into a one-dimensional array.
            byte[] pixels1d = new byte[Height * Width * 4];
            int index = 0;
            for (int h = 0; h < Height; h++)
                for (int w = 0; w < Width; w++)
                    for (int i = 0; i < 4; i++)
                        pixels1d[index++] = _buffer[h, w, i];

            // Update writeable bitmap with the colorArray to the image.
            Int32Rect rect = new Int32Rect(0, 0, Width, Height);
            int stride = 4 * Width;
            _bitmap.WritePixels(rect, pixels1d, stride, 0);
            return Task.CompletedTask;
        }

        public void SetPixel(int x, int y, InkyPixelColour colour)
        {
            var colourBytes = GetColor(colour);
            for (var c = 0; c < colourBytes.Length; c++)
                _buffer[y, x, c] = colourBytes[c];
        }
    }
}

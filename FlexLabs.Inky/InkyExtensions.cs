using System;

namespace FlexLabs.Inky
{
    /// <summary>
    /// Extension menthods for the inky driver
    /// </summary>
    public static class InkyExtensions
    {
        /// <summary>
        /// Try set the inky buffer pixel, as long as it's within the bounds of the screen
        /// </summary>
        /// <param name="inky">Inky driver</param>
        /// <param name="x">The x pixel position</param>
        /// <param name="y">The y pixel position</param>
        /// <param name="colour">The colour to set the pixel to</param>
        /// <returns>True if the pixel coordinates were within the bounds of the screen</returns>
        public static bool TrySetPixel(this IInky inky, int x, int y, InkyPixelColour colour)
        {
            if (x < 0 || y < 0 || x >= inky.Width || y >= inky.Height)
                return false;

            inky.SetPixel(x, y, colour);
            return true;
        }

        /// <summary>
        /// Draw a rectangle on the inky buffer
        /// </summary>
        /// <param name="inky">Inky driver</param>
        /// <param name="x1">The x pixel position of one corner</param>
        /// <param name="y1">The y pixel position of one corner</param>
        /// <param name="x2">The x pixel position of the opposite corner</param>
        /// <param name="y2">The y pixel position of the opposite corner</param>
        /// <param name="borderColour">The border colour of the rectangle</param>
        /// <param name="fillColour">The fill colour of the rectangle (optional)</param>
        public static void DrawRectangle(this IInky inky, int x1, int y1, int x2, int y2, InkyPixelColour borderColour, InkyPixelColour? fillColour = null)
        {
            for (var i = Math.Min(x1, x2); i <= Math.Max(x1, x2); i++)
            {
                inky.SetPixel(i, y1, borderColour);
                if (y1 != y2)
                    inky.SetPixel(i, y2, borderColour);
            }

            for (var i = Math.Min(y1, y2); i <= Math.Max(y1, y2); i++)
            {
                inky.SetPixel(x1, i, borderColour);
                if (x1 != x2)
                    inky.SetPixel(x2, i, borderColour);
            }

            if (fillColour.HasValue && Math.Abs(x2 - x1) > 1 && Math.Abs(y2 - y1) > 1)
            {
                for (var i = Math.Min(x1, x2) + 1; i < Math.Max(x1, x2); i++)
                    for (var j = Math.Min(y1, y2) + 1; j < Math.Max(y1, y2); j++)
                        inky.SetPixel(i, j, fillColour.Value);
            }
        }
    }
}

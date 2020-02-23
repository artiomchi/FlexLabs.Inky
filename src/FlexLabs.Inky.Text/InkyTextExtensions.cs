using System;
using System.Collections.Generic;

namespace FlexLabs.Inky.Text
{
    /// <summary>
    /// Inky extension methods used to write text onto the screen
    /// </summary>
    public static class InkyTextExtensions
    {
        /// <summary>
        /// Write a string to the screen
        /// </summary>
        /// <param name="inky">Inky driver</param>
        /// <param name="x">The x pixel position</param>
        /// <param name="y">The y pixel position</param>
        /// <param name="colour">The colour of the text</param>
        /// <param name="font">The font used</param>
        /// <param name="text">The text to be displayed</param>
        /// <param name="spacing">Default space width</param>
        /// <returns>The bottom right corner of the text</returns>
        public static (int x, int y) DrawString(this IInkyDriver inky, int x, int y, InkyPixelColour colour, Dictionary<char, bool[,]> font, string text, int spacing = 2)
        {
            if (inky is null)
                throw new ArgumentNullException(nameof(inky));
            if (font is null)
                throw new ArgumentNullException(nameof(font));

            if (text is null)
                return (x, y);

            var xp = x;
            var yp = y;
            for (var ci = 0; ci < text.Length; ci++)
            {
                var c = text[ci];
                if (!font.TryGetValue(c, out var map))
                    if (!font.TryGetValue('_', out map))
                        continue;

                if (c == ' ')
                {
                    xp += map.GetLength(0) / 3;
                    yp = y + map.GetLength(0);
                    continue;
                }

                for (int i = 0; i < map.GetLength(0); i++)
                    for (int j = 0; j < map.GetLength(1); j++)
                        if (map[i, j])
                            inky.TrySetPixel(xp + j, y + i, colour);

                xp += map.GetLength(1) + spacing;
                yp = y + map.GetLength(0);
            }
            return (xp, yp);
        }
    }
}

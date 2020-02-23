using System;
using System.Collections.Generic;

namespace FlexLabs.Inky.Text
{
    public static class InkyTextExtensions
    {
        public static (int x, int y) DrawString(this IInky inky, int x, int y, InkyPixelColour colour, Dictionary<char, bool[,]> font, string text, int spacing = 2)
        {
            if (text == null)
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

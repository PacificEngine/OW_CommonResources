using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources
{
    public class Shapes
    {
        private Color[] colors;
        public int width { get; }
        public int height { get; }

        public Shapes(int width, int height)
        {
            this.width = width;
            this.height = height;
            colors = Enumerable.Repeat(Color.clear, width * height).ToArray();
        }

        public void addDot(int centerX, int centerY, Color colorCenter, Color colorEdge, float radius)
        {
            // Could fix this by first drawing a quarter circle, and then imposing it on the underlying shape
            Color[] quarter = getQuarterCircle(colorCenter, colorEdge, radius);

            var size = (int)Math.Ceiling(radius + 1f);
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var color = quarter[x * size + y];
                    setColor(centerX + x, centerY + y, color);
                    setColor(centerX + x, centerY - y, color);
                    setColor(centerX - x, centerY + y, color);
                    setColor(centerX - x, centerY - y, color);
                }
            }
        }

        private Color[] getQuarterCircle(Color colorCenter, Color colorEdge, float radius)
        {
            float size = (float)Math.Ceiling(radius + 1f);
            Color[] quarterCircle = Enumerable.Repeat(Color.clear, (int)(size * size)).ToArray();
            for (float x = 0; x < (int)size; x++)
            {
                float y1 = (float)Math.Sqrt(radius * radius - x * x);
                float y2 = (float)Math.Sqrt(radius * radius - (x + 1) * (x + 1));

                float yTop = y1 - Math.Min(0, (y1 - y2) / 2f);
                float xPercentage = (radius - x) / radius;
                float yTopPercentage = yTop - (float)Math.Floor(yTop);

                var color = applyTransparency(colorEdge, (float)yTopPercentage);
                quarterCircle[(int)x * (int)(size) + (int)Math.Floor(y1)] = color;
                quarterCircle[(int)Math.Floor(y1) * (int)(size) + (int)x] = color;
                var zeroColor = getGradiant(colorCenter, colorEdge, xPercentage);
                for (float y = x; y < Math.Floor(yTop); y++)
                {
                    yTopPercentage = 1f - (yTop - (y + 0.5f)) / yTop;

                    color = getGradiant(colorEdge, zeroColor, (float)yTopPercentage);
                    quarterCircle[(int)x * (int)(size) + (int)y] = color;
                    quarterCircle[(int)y * (int)(size) + (int)x] = color;
                }
            }
            return quarterCircle;
        }

        public void drawLine(int x1, int y1, int x2, int y2, Color colorCenter, Color colorEdge, float width)
        {
            // Todo: Make this calculation work better, so we don't get gaps
            // Could do this by doing the calculation in reverse
            var radians = (float)Math.Atan2(y1 - y2, x1 - x2) + Math.PI;
            var rectWidth = (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
            var rectHeight = width / 2f;

            var cos = (float)Math.Cos(-1 * radians);
            var sin = (float)Math.Sin(-1 * radians);

            for (float x = 0f; x < rectWidth; x++)
            {
                for (float y = -1f * rectHeight; y < rectHeight; y++)
                {
                    var newX = x * cos + y * sin;
                    var newY = -1 * x * sin + y * cos;
                    setColor((int)Math.Round(newX) + x1, (int)Math.Round(newY) + y1, getGradiant(colorCenter, colorEdge, (rectHeight - Math.Abs(y)) / rectHeight));
                }
            }
        }

        public Texture2D getTexture()
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, colors[x * height + y]);
                }
            }
            texture.Apply();
            return texture;
        }

        private void setColor(int x, int y, Color color)
        {
            if (0 <= y && y < height && 0 <= x && x < width)
            {
                colors[x * height + y] = blendColors(color, colors[x * height + y]);
            }
        }


        private Color getColor(int x, int y)
        {
            if (0 <= y && y < height && 0 <= x && x < width)
            {
                return colors[x * height + y];
            }
            return Color.clear;
        }

        private Color applyTransparency(Color color1, float percentOpaque)
        {
            return new Color(color1.r, color1.g, color1.b, color1.a * percentOpaque);
        }

        private Color getGradiant(Color color1, Color color2, float percentColor1)
        {
            var percentColor2 = 1f - percentColor1;
            return new Color(color1.r * percentColor1 + percentColor2 * color2.r, color1.g * percentColor1 + percentColor2 * color2.g, color1.b * percentColor1 + percentColor2 * color2.b, color1.a * percentColor1 + percentColor2 * color2.a);
        }

        private Color blendColors(Color topColor, Color bottomColor)
        {
            var percentColor2 = 1f - topColor.a;
            if (percentColor2 == 0f)
            {
                return topColor;
            }
            else if (percentColor2 == 1f)
            {
                return bottomColor;
            }
            return new Color(topColor.r * topColor.a + percentColor2 * bottomColor.r, topColor.g * topColor.a + percentColor2 * bottomColor.g, topColor.b * topColor.a + percentColor2 * bottomColor.b, Math.Min(topColor.a, bottomColor.a) / Math.Max(topColor.a, bottomColor.a));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public class Shapes2D
    {
        private Color[] colors;
        public Vector2 size { get; }

        public Shapes2D(Vector2 size)
        {
            this.size = new Vector2((float)Math.Ceiling(size.x), (float)Math.Ceiling(size.y));
            colors = Enumerable.Repeat(Color.clear, (int)Math.Ceiling(size.x * size.y)).ToArray();
        }

        public void drawTexture(Texture2D texture, Vector2 placement)
        {
            for (float x = 0; 0 <= (x + placement.x) && x < (size.x + placement.x) && x < texture.width; x++)
            {
                for (float y = 0; 0 <= (y + placement.y) && y < (size.y + placement.y) && y < texture.height; y++)
                {
                    var newX = (int)(x + placement.x);
                    var newY = (int)(y + placement.y);
                    setColor(newX, newY, blendColors(getColor(newX, newY), texture.GetPixel((int)x, (int)y)));
                }
            }
        }

        public void drawCircle(Vector2 center, Color colorCenter, Color colorEdge, float radius)
        {
            // Could fix this by first drawing a quarter circle, and then imposing it on the underlying shape
            Color[] quarter = getQuarterCircle(colorCenter, colorEdge, radius);

            var size = (int)Math.Ceiling(radius + 1f);
            for (float x = 0; x < size; x++)
            {
                for (float y = 0; y < size; y++)
                {
                    var color = quarter[(int)(x * size + y)];
                    setColor((int)Math.Floor(center.x + x), (int)Math.Floor(center.y + y), color);
                    setColor((int)Math.Floor(center.x + x), (int)Math.Floor(center.y - y), color);
                    setColor((int)Math.Floor(center.x - x), (int)Math.Floor(center.y + y), color);
                    setColor((int)Math.Floor(center.x - x), (int)Math.Floor(center.y - y), color);
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
                var index = (int)Math.Floor(Math.Floor(x) * Math.Floor(size) + Math.Floor(y1));
                if (0 <= index && index < quarterCircle.Length)
                    quarterCircle[index] = color;

                index = (int)Math.Floor(Math.Floor(y1) * Math.Floor(size) + Math.Floor(x));
                if (0 <= index && index < quarterCircle.Length)
                    quarterCircle[index] = color;

                var zeroColor = getGradiant(colorCenter, colorEdge, xPercentage);
                for (float y = x; y < Math.Floor(yTop); y++)
                {
                    yTopPercentage = 1f - (yTop - (y + 0.5f)) / yTop;

                    color = getGradiant(colorEdge, zeroColor, (float)yTopPercentage);

                    index = (int)Math.Floor(Math.Floor(x) * Math.Floor(size) + Math.Floor(y));
                    if (0 <= index && index < quarterCircle.Length)
                        quarterCircle[index] = color;

                    index = (int)Math.Floor(Math.Floor(y) * Math.Floor(size) + Math.Floor(x));
                    if (0 <= index && index < quarterCircle.Length)
                        quarterCircle[index] = color;
                }
            }
            return quarterCircle;
        }

        public void drawRectangle(Vector2 a, Vector2 b, Color colorCenter, Color colorEdge, float width)
        {
            var radians = (float)Math.Atan2(a.y - b.y, a.x - b.x) + Math.PI;
            var rectWidth = (float)Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
            var rectHeight = width / 2f;

            var cos = (float)Math.Cos(-1f * radians);
            var sin = (float)Math.Sin(-1f * radians);

            // Top Left
            var minX = rectHeight * sin;
            var maxX = minX;
            var minY = rectHeight * cos;
            var maxY = minY;

            // Bottom Left
            var newX = -1 * rectHeight * sin;
            var newY = -1 * rectHeight * cos;

            minX = Math.Min(newX, minX);
            maxX = Math.Max(newX, maxX);
            minY = Math.Min(newY, minY);
            maxY = Math.Max(newY, maxY);

            // Top Right
            newX = rectWidth * cos + rectHeight * sin;
            newY = -1 * rectWidth * sin + rectHeight * cos;

            minX = Math.Min(newX, minX);
            maxX = Math.Max(newX, maxX);
            minY = Math.Min(newY, minY);
            maxY = Math.Max(newY, maxY);

            // Bottom Right
            newX = rectWidth * cos - rectHeight * sin;
            newY = -1 * rectWidth * sin - rectHeight * cos;

            minX = (float)Math.Floor(Math.Min(newX, minX));
            maxX = (float)Math.Ceiling(Math.Max(newX, maxX));
            minY = (float)Math.Floor(Math.Min(newY, minY));
            maxY = (float)Math.Ceiling(Math.Max(newY, maxY));


            var nCos = (float)Math.Cos(radians);
            var nSin = (float)Math.Sin(radians);
            var multiplier = 2f / (2f * sin + (float)Math.Cos(-2f * radians) + 1f);
            for (float x = minX; x < maxX; x++)
            {
                for (float y = minY; y < maxY; y++)
                {
                    // Could improve by doing some antialiasing, but I'm lazy, that was already too much hassle with the dots.
                    var oldX = x * nCos + y * nSin;
                    var oldY = -1 * x * nSin + y * nCos;
                    if (0 <= oldX && oldX <= rectWidth && -1 * rectHeight <= oldY && oldY <= rectHeight)
                    {
                        setColor((int)Math.Floor(Math.Round(x) + a.x), (int)Math.Floor(Math.Round(y) + a.y), getGradiant(colorCenter, colorEdge, (rectHeight - Math.Abs(oldY)) / rectHeight));
                    }
                }
            }
        }

        public Texture2D getTexture()
        {
            int width = (int)Math.Ceiling(size.x);
            int height = (int)Math.Ceiling(size.y);
            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var index = x * height + y;
                    if (0 <= index && index < colors.Length)
                        texture.SetPixel(x, y, colors[index]);
                    else
                        texture.SetPixel(x, y, Color.clear);
                }
            }
            texture.Apply();
            return texture;
        }

        private void setColor(int x, int y, Color color)
        {
            int width = (int)Math.Ceiling(size.x);
            int height = (int)Math.Ceiling(size.y);
            if (0 <= y && y < height && 0 <= x && x < width)
            {
                colors[x * height + y] = blendColors(color, colors[x * height + y]);
            }
        }


        private Color getColor(int x, int y)
        {
            int width = (int)Math.Ceiling(size.x);
            int height = (int)Math.Ceiling(size.y);
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

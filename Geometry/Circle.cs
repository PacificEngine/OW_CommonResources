using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public static class Circle
    {
        private static float normalizeAngle(float angle)
        {
            return ((angle % (2f * (float)Math.PI)) + (2f * (float)Math.PI)) % (2f * (float)Math.PI);
        }

        public static float getArcLength(float radius, float arcAngle)
        {
            return radius * normalizeAngle(arcAngle);
        }

        public static float getArcAngle(float radius, float arcLength)
        {
            return normalizeAngle(arcLength / radius);
        }

        public static float getChordLength(float radius, float arcAngle)
        {
            return ((float)Math.Sin(normalizeAngle(arcAngle) / 2f)) * radius * 2f;
        }

        public static float getChordAngle(float radius, float cordLength)
        {
            return normalizeAngle((float)Math.Asin(cordLength / (2f * radius)) * 2f);
        }

        public static float getPercentageAngle(float percentage)
        {
            return (2f * (float)Math.PI) * percentage;
        }

        public static float getPercentage(float arcAngle)
        {
            return normalizeAngle(arcAngle) / (2f * (float)Math.PI);
        }

        public static float getCountAngle(float count)
        {
            return (2f * (float)Math.PI) / count;
        }

        public static float getCount(float arcAngle)
        {
            return (2f * (float)Math.PI) / normalizeAngle(arcAngle);
        }

        public static Vector2 getPointOnCircle(ref Vector2 center, float radius, float arcAngle)
        {
            arcAngle = normalizeAngle(arcAngle);
            return new Vector2(radius * (float)Math.Cos(arcAngle) + center.x, radius * (float)Math.Sin(arcAngle) + center.y);
        }

    }
}

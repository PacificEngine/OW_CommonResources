using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public static class Circle
    {
        public static float getArcLength(float radius, float arcAngle)
        {
            return radius * Angle.toRadian(arcAngle);
        }

        public static float getArcAngle(float radius, float arcLength)
        {
            return Angle.toDegrees(arcLength / radius);
        }

        public static float getChordLength(float radius, float arcAngle)
        {
            return ((float)Math.Sin(Angle.toRadian(arcAngle) / 2f)) * radius * 2f;
        }

        public static float getChordAngle(float radius, float cordLength)
        {
            return Angle.toDegrees((float)Math.Asin(cordLength / (2f * radius)) * 2f);
        }

        public static float getPercentageAngle(float percentage)
        {
            return 360f * percentage;
        }

        public static float getPercentage(float arcAngle)
        {
            return Angle.toRadian(arcAngle) / 360f;
        }

        public static float getCountAngle(float count)
        {
            return 360f / count;
        }

        public static float getCount(float arcAngle)
        {
            return (2f * (float)Math.PI) / Angle.toRadian(arcAngle);
        }

        public static Vector2 getPointOnCircle(ref Vector2 center, float radius, float arcAngle)
        {
            arcAngle = Angle.toRadian(arcAngle);
            return new Vector2(radius * (float)Math.Cos(arcAngle) + center.x, radius * (float)Math.Sin(arcAngle) + center.y);
        }

    }
}

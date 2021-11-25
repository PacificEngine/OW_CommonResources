using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public static class Ellipse
    {
        public static float toPolar(Vector2 coordinates, Vector2 radius)
        {
            var polar = 0f;
            if (coordinates.y > coordinates.x)
                polar = (float)Math.Asin(coordinates.y / radius.y);
            else
                polar = (float)Math.Acos(coordinates.x / radius.x);
            polar = Angle.toDegrees(polar) % (90f);
            if (coordinates.x > 0 && coordinates.y >= 0) // Right
                return polar;
            if (coordinates.x <= 0 && coordinates.y > 0) // Up
                return polar + 90f;
            if (coordinates.x < 0 && coordinates.y <= 0) // Left
                return polar + 180f;
            else if (coordinates.x >= 0 && coordinates.y < 0) // Down
                return polar + 270f;
            return 0f;
        }

        public static Vector2 fromPolar(float polar, Vector2 radius)
        {
            polar = Angle.toRadian(polar);
            return new Vector2(radius.x * (float)Math.Cos(polar), radius.y * (float)Math.Sin(polar));
        }

        public static float getFocus(Vector2 radius)
        {
            return (float)Math.Sqrt(Math.Abs((radius.y * radius.y) - (radius.x * radius.x)));
        }

        public static float getFocus(float majorRadius, float eccentricity)
        {
            return majorRadius * eccentricity;
        }

        public static float getMinorRadius(float majorRadius, float focus)
        {
            return (float)Math.Sqrt(Math.Abs((majorRadius * majorRadius) - (focus * focus)));
        }

        public static float getMajorRadius(float minorRadius, float focus)
        {
            return (float)Math.Sqrt(Math.Abs((minorRadius * minorRadius) + (focus * focus)));
        }

        public static float getAxisRectum(float majorRadius, float eccentricity)
        {
            return majorRadius * (1f - (eccentricity * eccentricity));
        }

        public static Vector2 fromPolarToSlope(float polar, Vector2 radius)
        {
            polar = Angle.toRadian(polar);
            return new Vector2(-1f * radius.x * (float)Math.Sin(polar), radius.y * (float)Math.Cos(polar));
        }

        public static float getRadius(float polar, Vector2 radius)
        {
            var eccentricity = getEccentricity(radius);
            if (radius.y > radius.x)
                return getRadius(polar, radius.y, polar - 90f);
            return getRadius(polar, radius.x, polar);
        }

        public static float getRadius(float majorRadius, float eccentricity, float polar)
        {
            polar = Angle.toRadian(polar);
            return (majorRadius * (1f - Math.Abs(eccentricity * (float)Math.Sin(polar))));
        }

        public static float getEccentricity(Vector2 radius)
        {
            if (radius.y > radius.x)
                return (float)Math.Sqrt(1f - (radius.x * radius.x) / (radius.y * radius.y));
            return (float)Math.Sqrt(1f - (radius.y * radius.y) / (radius.x * radius.x));
        }
    }
}

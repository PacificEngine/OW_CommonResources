using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public static class Angle
    {
        private const double twoPid = 2d * Math.PI;
        private const float twoPif = (float)twoPid;

        public static float normalizeDegrees(float degrees)
        {
            return (((degrees % 360f) + 360f) % 360f);
        }

        public static float normalizeRadian(float radians)
        {
            return (((radians % twoPif) + twoPif) % twoPif);
        }

        public static float toDegrees(float radians)
        {
            return (normalizeRadian(radians) / twoPif) * 360f;
        }

        public static float toRadian(float degrees)
        {
            return (normalizeDegrees(degrees) / 360f) * twoPif;
        }

        public static double normalizeDegrees(double degrees)
        {
            return (((degrees % 360d) + 360d) % 360d);
        }

        public static double normalizeRadian(double radians)
        {
            return (((radians % twoPid) + twoPid) % twoPid);
        }

        public static double toDegrees(double radians)
        {
            return (normalizeRadian(radians) / twoPid) * 360d;
        }

        public static double toRadian(double degrees)
        {
            return (normalizeDegrees(degrees) / 360d) * twoPid;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public static class Angle
    {
        private const float twoPi = (2f * (float)Math.PI);

        public static float normalizeDegrees(float degrees)
        {
            return (((degrees % 360f) + 360f) % 360f);
        }

        public static float normalizeRadian(float radians)
        {
            return (((radians % twoPi) + twoPi) % twoPi);
        }

        public static float toDegrees(float radians)
        {
            return (normalizeRadian(radians) / twoPi) * 360f;
        }

        public static float toRadian(float degrees)
        {
            return (normalizeDegrees(degrees) / 360f) * twoPi;
        }
    }
}

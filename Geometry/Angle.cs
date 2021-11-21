using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public static class Angle
    {
        public static float toDegrees(float radians)
        {
            return (((radians % (2f * (float)Math.PI)) + (2f * (float)Math.PI)) % (2f * (float)Math.PI) / (2f * (float)Math.PI)) * 360f;
        }

        public static float toRadian(float degrees)
        {
            return (((degrees % (360f)) + (360f)) % (360f) / 360f) * (2f * (float)Math.PI);
        }
    }
}

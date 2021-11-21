using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public static class Sphere
    {
        private static float normalizeLatitude(float latitude)
        {
            return latitude > Math.PI ? ((float)Math.PI - (latitude - (float)Math.PI)) : latitude;
        }

        public static float getRadiusOnSphere(float latitude, float radius)
        {
            var percentage = (float)Math.Sin(normalizeLatitude(Angle.toRadian(latitude)) - ((float)Math.PI / 2f));
            return (float)Math.Sqrt(radius * radius - radius * radius * percentage * percentage);
        }

        public static Vector3 getPointOnSphere(ref Vector3 center, float longitude, float latitude, float radius)
        {
            longitude = Angle.toRadian(longitude) - (float)Math.PI;
            latitude = normalizeLatitude(Angle.toRadian(latitude)) - ((float)Math.PI / 2f);
            return new Vector3((-1f * radius * (float)Math.Cos(latitude) * (float)Math.Sin(longitude)) + center.x, (-1f * radius * (float)Math.Cos(latitude) * (float)Math.Cos(longitude)) + center.y, (radius * (float)Math.Sin(latitude)) + center.z);
        }
    }
}

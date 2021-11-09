using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public static class Coordinates
    {

        public static Vector3[] nearestPoint(Vector3 point, params Vector3[] points)
        {
            Array.Sort(points, (p1, p2) => ((point - p1).magnitude.CompareTo((point - p2).magnitude)));
            return points;
        }

        public static Vector3[] furthestPoint(Vector3 point, params Vector3[] points)
        {
            Array.Sort(points, (p1, p2) => ((point - p2).magnitude.CompareTo((point - p1).magnitude)));
            return points;
        }

        public static Vector3[] nearestPoints(params Vector3[] points)
        {
            var p1 = points[0];
            var p2 = points[0];
            var distance = float.MaxValue;

            for (int i = 0; i < points.Length; i++)
            {
                for (int j = i + 1; j < points.Length; j++)
                {
                    var mag = (points[i] - points[j]).magnitude;
                    if (mag < distance)
                    {
                        p1 = points[i];
                        p2 = points[j];
                        distance = mag;
                    }
                }
            }

            return new Vector3[] { p1, p2 };
        }

        public static Vector3[] furthestPoints(params Vector3[] points)
        {
            var p1 = points[0];
            var p2 = points[0];
            var distance = 0f;

            for (int i = 0; i < points.Length; i++)
            {
                for (int j = i + 1; j < points.Length; j++)
                {
                    var mag = (points[i] - points[j]).magnitude;
                    if (mag > distance)
                    {
                        p1 = points[i];
                        p2 = points[j];
                        distance = mag;
                    }
                }
            }

            return new Vector3[] { p1, p2 };
        }

        public static float angle(Vector2 p1, Vector2 p2)
        {
            var v1 = p1; // / p1.magnitude;
            var v2 = p2; // / p2.magnitude;
            var value = Math.Atan2(v1.x * v2.y - v1.y * v2.x, v1.x * v2.x + v1.y * v2.y); // Math.Acos((p1.x * p2.x + p1.y * p2.y) / (p1.magnitude * p2.magnitude));
            if (double.IsNaN(value))
            {
                return 0f;
            }
            return (float)value;
        }

        public static float angle(Vector3 p1, Vector3 p2)
        {
            var value = Math.Acos((p1.x * p2.x + p1.y * p2.y + p1.z * p2.z) / (p1.magnitude * p2.magnitude));
            if (double.IsNaN(value))
            {
                return 0f;
            }
            return (float)value;
        }

        private static float angleX(Vector3 p1, Vector3 p2)
        {
            return angle(new Vector2(p1.y, p1.z), new Vector2(p2.y, p2.z));
        }

        private static float angleY(Vector3 p1, Vector3 p2)
        {
            return angle(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z));
        }

        private static float angleZ(Vector3 p1, Vector3 p2)
        {
            return angle(new Vector2(p1.x, p1.y), new Vector2(p2.x, p2.y));
        }

        public static Vector3 angleXYZ(Vector3 p1, Vector3 p2)
        {
            return new Vector3(angleX(p1, p2), angleY(p1, p2), angleZ(p1, p2));
        }

        private static Vector3 rotatePointX(ref Vector3 point, float xRotation)
        {
            var sin = (float)Math.Sin(xRotation);
            var cos = (float)Math.Cos(xRotation);
            return new Vector3(point.x, point.y * cos - point.z * sin, point.y * sin + point.z * cos);
        }

        private static Vector3 rotatePointY(ref Vector3 point, float yRotation)
        {
            var sin = (float)Math.Sin(yRotation);
            var cos = (float)Math.Cos(yRotation);
            return new Vector3(point.x * cos + point.z * sin, point.y, -1 * point.x * sin + point.z * cos);
        }

        private static Vector3 rotatePointZ(ref Vector3 point, float zRotation)
        {
            var sin = (float)Math.Sin(zRotation);
            var cos = (float)Math.Cos(zRotation);
            return new Vector3(point.x * cos - point.y * sin, point.x * sin + point.y * cos, point.z);
        }

        public static Vector3 rotatePoint(Vector3 point, Vector3 rotation)
        {
            return rotatePoint(ref point, ref rotation);
        }

        public static Vector3 rotatePoint(ref Vector3 point, ref Vector3 rotation)
        {
            var rotate = point;
            rotate = Coordinates.rotatePointX(ref rotate, rotation.x);
            rotate = Coordinates.rotatePointY(ref rotate, rotation.y);
            rotate = Coordinates.rotatePointZ(ref rotate, rotation.z);
            return rotate;
        }

        public static Vector3 getPointOnSphere(ref Vector3 center, float latitude, float longitude, float radius)
        {
            return new Vector3((radius * (float)Math.Cos(longitude) * (float)Math.Cos(latitude)) + center.x, (radius * (float)Math.Cos(latitude) * (float)Math.Sin(longitude)) + center.y, (radius * (float)Math.Sin(latitude)) + center.z);
        }
    }
}

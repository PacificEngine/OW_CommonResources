using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public class Shapes3D
    {
        private Dictionary<Vector3, int> verticeIndex;
        private Dictionary<int, Vector3> indexVectice;
        private List<Vector3> _vertices;
        private List<int> _triangles;

        public Vector3[] vertices { get { return _vertices.ToArray(); } }
        public int[] triangles { get { return _triangles.ToArray(); } }

        public Shapes3D()
        {
            verticeIndex = new Dictionary<Vector3, int>();
            indexVectice = new Dictionary<int, Vector3>();
            _vertices = new List<Vector3>();
            _triangles = new List<int>();
        }

        public Mesh getMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.Optimize();
            mesh.RecalculateNormals();

            return mesh;
        }

        public void drawBox(Vector3 start, Vector2 startSize, Vector3 end, Vector2 endSize, float startRotation, float endRotation)
        {
            var difference = end - start;
            if (difference.x < 0f && difference.y < 0f || difference.x < 0f && difference.z < 0f || difference.y < 0f && difference.z < 0f)
            {
                drawBox(end, endSize, start, startSize, endRotation, startRotation);
                return;
            }

            var angles = Coordinates.angleXYZ(difference, Vector3.forward);

            int[] vertices = {
                addVertex(start + Coordinates.rotatePoint(Coordinates.rotatePoint(new Vector3 (startSize.x / -2f, startSize.y / -2f, 0f), Vector3.forward * startRotation), angles)), // 0, 0, 0 (0)
                addVertex(start + Coordinates.rotatePoint(Coordinates.rotatePoint(new Vector3 (startSize.x / -2f, startSize.y / 2f, 0f), Vector3.forward * startRotation), angles)),  // 1, 0, 0 (1)
                addVertex(start + Coordinates.rotatePoint(Coordinates.rotatePoint(new Vector3 (startSize.x / 2f, startSize.y / -2f, 0f), Vector3.forward * startRotation), angles)),  // 0, 1, 0 (2)
                addVertex(start + Coordinates.rotatePoint(Coordinates.rotatePoint(new Vector3 (startSize.x / 2f, startSize.y / 2f, 0f), Vector3.forward * startRotation), angles)),   // 1, 1, 0 (3)
                addVertex(end + Coordinates.rotatePoint(Coordinates.rotatePoint(new Vector3 (endSize.x / -2f, endSize.y / -2f, 0f), Vector3.forward * endRotation), angles)),         // 0, 0, 1 (4)
                addVertex(end + Coordinates.rotatePoint(Coordinates.rotatePoint(new Vector3 (endSize.x / -2f, endSize.y / 2f, 0f), Vector3.forward * endRotation), angles)),          // 1, 0, 1 (5)
                addVertex(end + Coordinates.rotatePoint(Coordinates.rotatePoint(new Vector3 (endSize.x / 2f, endSize.y / -2f, 0f), Vector3.forward * endRotation), angles)),          // 0, 1, 1 (6)
                addVertex(end + Coordinates.rotatePoint(Coordinates.rotatePoint(new Vector3 (endSize.x / 2f, endSize.y / 2f, 0f), Vector3.forward * endRotation), angles)),           // 1, 1, 1 (7)
            };
            addRectangle(vertices[0], vertices[1], vertices[3], vertices[2], true);    // (x, x, 0)
            addRectangle(vertices[4], vertices[5], vertices[7], vertices[6], false);   // (x, x, 1)
            addRectangle(vertices[0], vertices[2], vertices[6], vertices[4], true);    // (0, x, x)
            addRectangle(vertices[1], vertices[3], vertices[7], vertices[5], false);   // (1, x, x)
            addRectangle(vertices[0], vertices[1], vertices[5], vertices[4], false);   // (x, 0, x)
            addRectangle(vertices[2], vertices[3], vertices[7], vertices[6], true);    // (x, 1, x)
        }

        public void drawCylinder(Vector3 from, Vector3 to, float startRadius, float endRadius)
        {

        }

        public void drawSphere(Vector3 center, float radius, int levels)
        {
            var root3 = (float)Math.Sqrt(3f);
            var length = 0.25f * (root3 * (float)Math.Sqrt((16f * radius * radius) + (9f * levels * levels)) - (3f * root3 * levels));
            var topHeight = radius - (float)Math.Sqrt((radius * radius) - ((length * length) / 3f));
            var topLongitude = topHeight / radius * ((float)Math.PI / 2f);
            var bottomLongitude = -1f * topLongitude;
            var topTeirs = new int[(levels + 1) / 2][];
            var bottomTeirs = new int[(levels + 1)/ 2][];
            var topRotation = 0f;
            var bottomRotation = 0f;
            if (levels % 2 == 1)
            {
                topRotation = (float)Math.PI / 3f;
            }
            topTeirs[0] = addVertices(Coordinates.getPointOnSphere(ref center, 0 + topRotation, topLongitude, radius), Coordinates.getPointOnSphere(ref center, (((float)Math.PI * 2f) / 3f) + topRotation, topLongitude, radius), Coordinates.getPointOnSphere(ref center, (((float)Math.PI * 4f) / 3f) + topRotation, topLongitude, radius));
            bottomTeirs[0] = addVertices(Coordinates.getPointOnSphere(ref center, 0 + bottomRotation, bottomLongitude, radius), Coordinates.getPointOnSphere(ref center, (((float)Math.PI * 2f) / 3f) + bottomRotation, bottomLongitude, radius), Coordinates.getPointOnSphere(ref center, (((float)Math.PI * 4f) / 3f) + bottomRotation, bottomLongitude, radius));
            addTriangle(topTeirs[0][0], topTeirs[0][1], topTeirs[0][2], true);
            addTriangle(bottomTeirs[0][0], bottomTeirs[0][1], bottomTeirs[0][2], true);

            // TODO, solve the inebetween
            var increment = (length * root3) / 2f;

            var top = topTeirs[topTeirs.Length - 1];
            var bottom = bottomTeirs[bottomTeirs.Length - 1];

            if (levels % 2 == 1)
            {
                for (int i = 0; i < top.Length; i++)
                {
                    addTriangle(top[i], top[(i + 1) % top.Length], bottom[i], false);
                    addTriangle(bottom[i], bottom[(i + 1) % top.Length], top[i], true);
                }
            }
            else
            {
                // TODO: Add a middle tier and connect them
            }
        }

        /*public void drawSphere(Vector3 center, float radius, Vector2 steps)
        {
            float pi2 = (float)(2f * Math.PI);
            float pi = (float)(Math.PI);

            float latitude_increment = (float)(pi2 / steps.x);
            float longitude_increment = (float)(pi / steps.y);
            //float longitudeReduction = 1f; // TODO

            // Create top and bottom points
            var top = getPointOnCircle(ref center, 0, pi, radius);
            vertices.Add(top);
            verticeIndex.Add(top, vertices.Count - 1);

            var bottom = getPointOnCircle(ref center, 0, 0, radius);
            vertices.Add(bottom);
            verticeIndex.Add(bottom, vertices.Count - 1);

            for (float longitude = longitude_increment; longitude < (pi - longitude_increment); longitude += longitude_increment)
            {
                var first = getPointOnCircle(ref center, 0, longitude, radius);
                vertices.Add(first);
                verticeIndex.Add(first, vertices.Count - 1);
                for (float latitude = latitude_increment; latitude < pi2; latitude += latitude_increment)
                {
                    var current = getPointOnCircle(ref center, latitude, longitude, radius);
                    vertices.Add(current);
                    verticeIndex.Add(current, vertices.Count - 1);

                    var previous = getPointOnCircle(ref center, latitude - latitude_increment, longitude, radius);
                    var below = getPointOnCircle(ref center, latitude, longitude - longitude_increment, radius);
                    var belowPrevious = getPointOnCircle(ref center, latitude - latitude_increment, longitude, radius);

                    triangles.Add(verticeIndex[current]);
                    triangles.Add(verticeIndex[previous]);
                    triangles.Add(verticeIndex[belowPrevious]);

                    triangles.Add(verticeIndex[current]);
                    triangles.Add(verticeIndex[below]);
                    triangles.Add(verticeIndex[belowPrevious]);
                }

                // Need a more effient method
            }
        }*/

        private int[] addVertices(params Vector3[] vertices)
        {
            int[] values = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                values[i] = addVertex(vertices[i]);
            }
            return values;
        }

        private int addVertex(Vector3 vertex)
        {
            return addVertex(ref vertex);
        }

        private int addVertex(ref Vector3 vertex)
        {
            int value = 0;
            if (verticeIndex.TryGetValue(vertex, out value))
            {
                return value;
            }
            else
            {
                _vertices.Add(vertex);
                indexVectice.Add(_vertices.Count - 1, vertex);
                verticeIndex.Add(vertex, _vertices.Count - 1);
                return _vertices.Count - 1;
            }
        }


        private void addRectangle(ref Vector3 p1, ref Vector3 p2, ref Vector3 p3, ref Vector3 p4, bool clockwise)
        {
            addRectangle(addVertex(ref p1), addVertex(ref p2), addVertex(ref p3), addVertex(ref p4), clockwise);
        }

        private void addRectangle(int v1, int v2, int v3, int v4, bool clockwise)
        {
            var p1 = indexVectice[v1];
            var p2 = indexVectice[v2];
            var p3 = indexVectice[v3];
            var p4 = indexVectice[v4];

            addTriangle(v1, v2, v3, clockwise);
            addTriangle(v1, v3, v4, clockwise);
        }

        private void addTriangle(ref Vector3 p1, ref Vector3 p2, ref Vector3 p3, bool clockwise)
        {
            addTriangle(addVertex(ref p1), addVertex(ref p2), addVertex(ref p3), clockwise);
        }

        private void addTriangle(int v1, int v2, int v3, bool clockwise)
        {
            if (clockwise)
            {
                _triangles.Add(v1);
                _triangles.Add(v2);
                _triangles.Add(v3);
            }
            else
            {
                _triangles.Add(v3);
                _triangles.Add(v2);
                _triangles.Add(v1);
            }
        }
    }
}

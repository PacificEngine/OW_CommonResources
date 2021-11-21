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

        private Vector3 getRotation(Vector3 start, Vector3 end)
        {
            bool flipX = false;
            bool flipY = false;
            bool flipZ = false;

            var difference = end - start;
            if (difference.z > 0f)
            {
                return getRotation(end, start);
            }

            if (difference.x <= 0f && difference.y <= 0f)
            {
                flipX = true;
                flipY = true;
                difference = new Vector3(-1f * difference.x, -1f * difference.y, difference.z);
            }

            var angles = Coordinates.angleXYZ(difference, Vector3.forward);
            if (difference.x != 0f && difference.z == 0f)
            {
                angles = angles + Coordinates.angleXYZ(difference, Vector3.right);
            }

            if (flipX)
            {
                difference = new Vector3(-1f * difference.x, difference.y, difference.z);
                angles = new Vector3(-1f * angles.x, angles.y, angles.z);
            }
            if (flipY)
            {
                difference = new Vector3(difference.x, -1f * difference.y, difference.z);
                angles = new Vector3(angles.x, -1f * angles.y, angles.z);
            }
            if (flipZ)
            {
                difference = new Vector3(difference.x, difference.y, -1f * difference.z);
                angles = new Vector3(angles.x, angles.y, -1f * angles.z);
            }

            return angles;

        }

        private Vector3 calculateRotation(Vector3 point, float rotation, Vector3 angle)
        {
            return Coordinates.rotatePoint(Coordinates.rotatePoint(point, Vector3.forward * rotation), angle);
        }

        public void drawBox(Vector3 start, Vector2 startSize, Vector3 end, Vector2 endSize, float startRotation, float endRotation)
        {
            var difference = end - start;
            if (difference.z > 0f)
            {
                drawBox(end, endSize, start, startSize, endRotation, startRotation);
                return;
            }
            bool normal = !(difference.y == 0f && difference.z == 0f) && !(difference.x < 0f && difference.y > 0f && difference.z == 0f);

            Vector3 angles = getRotation(start, end);

            int[] vertices = {
                addVertex(start + calculateRotation(new Vector3 (startSize.x / -2f, startSize.y / -2f, 0f), startRotation, angles)), // 0, 0, 0 (0)
                addVertex(start + calculateRotation(new Vector3 (startSize.x / -2f, startSize.y /  2f, 0f), startRotation, angles)), // 1, 0, 0 (1)
                addVertex(start + calculateRotation(new Vector3 (startSize.x /  2f, startSize.y / -2f, 0f), startRotation, angles)), // 0, 1, 0 (2)
                addVertex(start + calculateRotation(new Vector3 (startSize.x /  2f, startSize.y /  2f, 0f), startRotation, angles)), // 1, 1, 0 (3)
                addVertex(end + calculateRotation(new Vector3 (endSize.x / -2f, endSize.y / -2f, 0f), endRotation, angles)),         // 0, 0, 1 (0)
                addVertex(end + calculateRotation(new Vector3 (endSize.x / -2f, endSize.y /  2f, 0f), endRotation, angles)),         // 1, 0, 1 (1)
                addVertex(end + calculateRotation(new Vector3 (endSize.x /  2f, endSize.y / -2f, 0f), endRotation, angles)),         // 0, 1, 1 (2)
                addVertex(end + calculateRotation(new Vector3 (endSize.x /  2f, endSize.y /  2f, 0f), endRotation, angles))          // 1, 1, 1 (3)
            };

            addRectangle(vertices[0], vertices[1], vertices[3], vertices[2], !normal);    // (x, x, 0)
            addRectangle(vertices[4], vertices[5], vertices[7], vertices[6], normal);     // (x, x, 1)
            addRectangle(vertices[0], vertices[2], vertices[6], vertices[4], !normal);    // (0, x, x)
            addRectangle(vertices[1], vertices[3], vertices[7], vertices[5], normal);     // (1, x, x)
            addRectangle(vertices[2], vertices[3], vertices[7], vertices[6], !normal);    // (x, 1, x)
            addRectangle(vertices[0], vertices[1], vertices[5], vertices[4], normal);     // (x, 0, x)
        }

        public void drawCylinder(Vector3 start, Vector3 end, float startRadius, float endRadius, int sides)
        {
            var difference = end - start;
            if (difference.z > 0f)
            {
                drawCylinder(end, start, endRadius, startRadius, sides);
                return;
            }
            bool normal = !(difference.y == 0f && difference.z == 0f) && !(difference.x < 0f && difference.y > 0f && difference.z == 0f);

            Vector3 angles = getRotation(start, end);

            var zero = Vector2.zero;

            int top = addVertex(start);
            int bottom = addVertex(end);
            int[] topVertices = new int[sides];
            int[] bottomVertices = new int[sides];

            var vertex = Circle.getPointOnCircle(ref zero, startRadius, 0);
            topVertices[0] = addVertex(start + calculateRotation(new Vector3(vertex.x, vertex.y, 0f), 0, angles));

            vertex = Circle.getPointOnCircle(ref zero, endRadius, 0);
            bottomVertices[0] = addVertex(end + calculateRotation(new Vector3(vertex.x, vertex.y, 0f), 0, angles));

            var increment = 360f / (float)sides;
            for (int i = 1; i < sides; i++)
            {
                vertex = Circle.getPointOnCircle(ref zero, startRadius, increment * (float)i);
                topVertices[i] = addVertex(start + calculateRotation(new Vector3(vertex.x, vertex.y, 0f), 0, angles));

                vertex = Circle.getPointOnCircle(ref zero, endRadius, increment * (float)i);
                bottomVertices[i] = addVertex(end + calculateRotation(new Vector3(vertex.x, vertex.y, 0f), 0, angles));

                addTriangle(topVertices[i], topVertices[i - 1], top, !normal);
                addTriangle(bottomVertices[i], bottomVertices[i - 1], bottom, normal);
                addRectangle(topVertices[i], bottomVertices[i], bottomVertices[i - 1], topVertices[i - 1], !normal);
            }

            addTriangle(topVertices[0], topVertices[sides - 1], top, !normal);
            addTriangle(bottomVertices[0], bottomVertices[sides - 1], bottom, normal);
            addRectangle(topVertices[0], bottomVertices[0], bottomVertices[sides - 1], topVertices[sides - 1], !normal);
        }

        public void drawSphere(Vector3 center, float radius, int levels)
        {
            var root3 = (float)Math.Sqrt(3f);
            var latIncrement = Circle.getCountAngle(levels * 2);
            var chordLength = Circle.getChordLength(radius, latIncrement);
            var length = chordLength * root3 / 2f;
            var height = radius - (float)Math.Sqrt(radius * radius - 0.25f * chordLength * chordLength);
            var topLatitude = (float)(180f) - (height / radius);
            var bottomLattiude = (float)180f - topLatitude;

            var totalLevels = (int)Math.Ceiling((topLatitude / latIncrement));
            var topTeirs = new Vector3[totalLevels / 2][];
            var bottomTeirs = new Vector3[totalLevels / 2][];
            var isTopRotated = totalLevels % 2 == 1;
            var isBottomRotated = false;
            topTeirs[0] = makeASphereTierViaAngle(ref center, topLatitude, radius, 120f, isTopRotated);
            bottomTeirs[0] = makeASphereTierViaAngle(ref center, bottomLattiude, radius, 120f, isBottomRotated);

            addTriangle(ref topTeirs[0][0], ref topTeirs[0][1], ref topTeirs[0][2], false);
            addTriangle(ref bottomTeirs[0][0], ref bottomTeirs[0][1], ref bottomTeirs[0][2], true);
            
            var level = 1;
            for (level = 1; level < topTeirs.Length; level++)
            {
                isTopRotated = !isTopRotated;
                isBottomRotated = !isBottomRotated;

                topTeirs[level] = makeASphereTierViaLength(ref center, topLatitude - (float)level * latIncrement, radius, length, isTopRotated);
                bottomTeirs[level] = makeASphereTierViaLength(ref center, bottomLattiude + (float)level * latIncrement, radius, length, isBottomRotated);

                connectTwoSphereTeirs(ref topTeirs[level], ref topTeirs[level - 1], false);
                connectTwoSphereTeirs(ref bottomTeirs[level], ref bottomTeirs[level - 1], true);
            }

            var currentTier = topTeirs[topTeirs.Length - 1];
            var lastTier = bottomTeirs[bottomTeirs.Length - 1];
            if (totalLevels % 2 == 1)
            {
                currentTier = makeASphereTierViaLength(ref center, 90f, radius, length, false);

                connectTwoSphereTeirs(ref currentTier, ref topTeirs[topTeirs.Length - 1], false);
            }

            connectTwoSphereTeirs(ref currentTier, ref lastTier, true);
        }

        private Vector3[] makeASphereTierViaLength(ref Vector3 center, float latitude, float radius, float length, bool isRotated)
        {
            var rad = Sphere.getRadiusOnSphere(latitude, radius);
            var arcAngle = Circle.getChordAngle(rad, length);

            return makeASphereTierViaAngle(ref center, latitude, radius, arcAngle, isRotated);
        }

        private Vector3[] makeASphereTierViaAngle(ref Vector3 center, float latitude, float radius, float arcAngle, bool isRotated)
        {
            var count = (int)Math.Ceiling(Circle.getCount(arcAngle));
            arcAngle = Circle.getCountAngle(count);

            var rotation = isRotated ? (arcAngle / 2f) : 0f;
            var currentTier = new Vector3[count];
            for (int j = 0; j < count; j++)
            {
                currentTier[j] = Sphere.getPointOnSphere(ref center, ((float)j * arcAngle + rotation) - 180f, latitude, radius);
            }

            return currentTier;
        }

        private void connectTwoSphereTeirs(ref Vector3[] currentTier, ref Vector3[] lastTier, bool normal)
        {
            int lastIndex = 0;
            float distance = float.MaxValue;
            for (int i = 0; i < lastTier.Length; i++)
            {
                var d2 = (currentTier[0] - lastTier[0]).sqrMagnitude;
                if (d2 < distance)
                {
                    lastIndex = i;
                    distance = d2;
                }
            }
            int firstIndex = lastIndex;


            for (int j = 1; j < currentTier.Length; j++)
            {
                int nextIndex = (lastIndex + 1) % lastTier.Length;
                if ((currentTier[j] - lastTier[lastIndex]).sqrMagnitude > (currentTier[j] - lastTier[nextIndex]).sqrMagnitude)
                {
                    addTriangle(ref currentTier[j], ref lastTier[nextIndex], ref lastTier[lastIndex], normal);
                    addTriangle(ref currentTier[j], ref currentTier[j - 1], ref lastTier[lastIndex], !normal);
                    lastIndex = nextIndex;
                }
                else
                {
                    addTriangle(ref currentTier[j], ref currentTier[j - 1], ref lastTier[lastIndex], !normal);
                }
            }

            if (firstIndex != lastIndex)
            {
                addTriangle(ref currentTier[0], ref lastTier[firstIndex], ref lastTier[lastIndex], normal);
                addTriangle(ref currentTier[0], ref currentTier[currentTier.Length - 1], ref lastTier[lastIndex], !normal);
            }
            else
            {
                addTriangle(ref currentTier[0], ref currentTier[currentTier.Length - 1], ref lastTier[lastIndex], !normal);
            }
        }

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

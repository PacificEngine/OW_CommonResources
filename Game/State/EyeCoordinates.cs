using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Geometry;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PacificEngine.OW_CommonResources.Game.State
{
    public static class EyeCoordinates
    {
        private static KeyInfoPromptController keyInfoPromptController = null;
        private static NomaiCoordinateInterface nomaiCoordinateInterface = null;
        private static ScreenPromptElement eyePromptElement = null;
        private static Hologram eyeHologram = null;
        private static ShipLogFactListItem eyeFactBubble = null;
        private static System.Random random = new System.Random();

        private static int[] _x = new int[] { 1, 5, 4 };
        private static int[] _y = new int[] { 3, 0, 1, 4 };
        private static int[] _z = new int[] { 1, 2, 3, 0, 5, 4 };

        private static int[] x
        {
            get
            {
                return nomaiCoordinateInterface?.GetValue<int[]>("_coordinateX") ?? _x;
            }
            set
            {
                _x = value;
                nomaiCoordinateInterface?.SetValue("_coordinateX", value);
            }
        }

        private static int[] y
        {
            get
            {
                return nomaiCoordinateInterface?.GetValue<int[]>("_coordinateY") ?? _y;
            }
            set
            {
                _y = value;
                nomaiCoordinateInterface?.SetValue("_coordinateY", value);
            }
        }

        private static int[] z
        {
            get
            {
                return nomaiCoordinateInterface?.GetValue<int[]>("_coordinateZ") ?? _z;
            }
            set
            {
                _z = value;
                nomaiCoordinateInterface?.SetValue("_coordinateZ", value);
            }
        }

        public static void Start()
        {
            Helper.helper.HarmonyHelper.AddPrefix<NomaiCoordinateInterface>("Awake", typeof(EyeCoordinates), "onNomaiCoordinateInterfaceAwake");
            Helper.helper.HarmonyHelper.AddPostfix<KeyInfoPromptController>("Start", typeof(EyeCoordinates), "onKeyInfoPromptControllerStart");
            Helper.helper.HarmonyHelper.AddPostfix<OrbitalCannonHologramProjector>("Awake", typeof(EyeCoordinates), "onOrbitalCannonHologramProjectorAwake");
            Helper.helper.HarmonyHelper.AddPostfix<ShipLogFactListItem>("Start", typeof(EyeCoordinates), "onShipLogFactListItemStart");
            //ShipLogFactListItem
        }

        public static void Awake()
        {
        }

        public static void Destroy()
        {
        }


        public static void Update()
        {
        }

        public static void randomizeCoordinates(System.Random random)
        {
            setCoordinates(generateCoordinate(ref random), generateCoordinate(ref random), generateCoordinate(ref random));
        }

        public static void setCoordinates(int[] x, int[] y, int[] z)
        {
            EyeCoordinates.x = x;
            EyeCoordinates.y = y;
            EyeCoordinates.z = z;
            updateCoordinates();
        }

        public static void updateCoordinates()
        {
            if (keyInfoPromptController)
            {
                var manager = Locator.GetPromptManager();
                var oldPrompt = keyInfoPromptController.GetValue<ScreenPrompt>("_eyeCoordinatesPrompt");
                manager.RemoveScreenPrompt(oldPrompt);
                var texture = getCoordinatesImage().getTexture();
                var eyePrompt = new ScreenPrompt(UITextLibrary.GetString(UITextType.EyeCoordinates) + "<EYE>", Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero));
                eyePromptElement = manager.AddScreenPrompt(eyePrompt, manager.GetScreenPromptList(PromptPosition.LowerLeft), manager.GetTextAnchor(PromptPosition.LowerLeft), -1, oldPrompt.IsVisible());
                keyInfoPromptController.SetValue("_eyeCoordinatesPrompt", eyePrompt);
            }

            if (eyeHologram)
            {
                var model = getCoordinatesModel();
                var gameObject = eyeHologram.GetComponentInChildren<MeshRenderer>().gameObject;
                gameObject.DestroyAllComponentsImmediate<MeshFilter>();
                var filter = gameObject.AddComponent<MeshFilter>();
                filter.mesh = model.getMesh();
            }
        }

        public static Shapes2D getCoordinatesImage()
        {
            var x = getCoordinate2D(EyeCoordinates.x);
            var y = getCoordinate2D(EyeCoordinates.y);
            var z = getCoordinate2D(EyeCoordinates.z);
            return drawCoordinate(ref x, ref y, ref z);
        }

        public static Shapes3D getCoordinatesModel()
        {
            var x = getCoordinate3D(EyeCoordinates.x);
            var y = getCoordinate3D(EyeCoordinates.y);
            var z = getCoordinate3D(EyeCoordinates.z);
            return drawCoordinate(ref x, ref y, ref z);
        }

        private static bool onNomaiCoordinateInterfaceAwake(ref NomaiCoordinateInterface __instance)
        {
            EyeCoordinates.nomaiCoordinateInterface = __instance;
            EyeCoordinates.setCoordinates(_x, _y, _z);
            return true;
        }

        private static void onKeyInfoPromptControllerStart(ref KeyInfoPromptController __instance)
        {
            keyInfoPromptController = __instance;
            EyeCoordinates.updateCoordinates();
        }


        private static void onOrbitalCannonHologramProjectorAwake(ref OrbitalCannonHologramProjector __instance)
        {
            var holograms = ((OrbitalCannonHologramProjector)__instance).GetValue<GameObject[]>("_holograms");
            foreach (GameObject hologram in holograms)
            {
                if ("Hologram_EyeCoordinates".Equals(hologram.name))
                {
                    eyeHologram = hologram.GetComponent<Hologram>();
                    break;
                }
            }
            EyeCoordinates.updateCoordinates();
        }


        private static void onShipLogFactListItemStart(ref ShipLogFactListItem __instance)
        {
            Helper.helper.Console.WriteLine("Fact" + __instance);
           // EyeCoordinates.updateCoordinates();
        }

        private static Shapes3D drawCoordinate(ref Vector3[] x, ref Vector3[] y, ref Vector3[] z)
        {
            var coordinates = new Shapes3D();
            drawCoordinate(ref coordinates, ref x, 1.25f, 0.5f, 0.25f);
            drawCoordinate(ref coordinates, ref y, -1.25f, 0.5f, 0.25f);
            drawCoordinate(ref coordinates, ref z, -3.75f, 0.5f, 0.25f);
            return coordinates;
        }

        private static void drawCoordinate(ref Shapes3D coordinates, ref Vector3[] w, float xOffset, float height, float width)
        {
            var multiplier = height / 2.5f;
            if (w.Length > 0)
            {
                var x = (w[0].x + xOffset) * multiplier;
                var y = (w[0].y) * multiplier;
                var z = (w[0].z) * multiplier;
                //coordinates.drawSphere(new Vector3(x, y, z), width * (multiplier / 2f), 1);
            }

            for (int i = 1; i < w.Length; i++)
            {
                var x1 = (w[i].x + xOffset) * multiplier;
                var y1 = (w[i].y) * multiplier;
                var z1 = (w[i].z) * multiplier;
                var x2 = (w[i - 1].x + xOffset) * multiplier;
                var y2 = (w[i - 1].y) * multiplier;
                var z2 = (w[i - 1].z) * multiplier;
                //coordinates.drawSphere(new Vector3(x1, y1, z1), width * (multiplier / 2f), 1);
                coordinates.drawBox(new Vector3(x1, y1, z1), new Vector2(width * (multiplier / 2f), width * (multiplier / 2f)), new Vector3(x2, y2, z2), new Vector2(width * (multiplier / 2f), width * (multiplier / 2f)), 0f, 0f);
            }
            coordinates.drawBox(Vector3.one, new Vector2(0.125f, 0.125f), Vector3.one + Vector3.right, new Vector2(0.125f, 0.125f), 0f, 0f);
            coordinates.drawBox(Vector3.one, new Vector2(0.50f, 0.50f), Vector3.one + Vector3.up, new Vector2(0.50f, 0.50f), 0f, 0f);
            coordinates.drawBox(Vector3.one, new Vector2(0.25f, 0.25f), Vector3.one + Vector3.forward, new Vector2(0.25f, 0.25f), 0f, 0f);

            // Q1 = Right, Up, Forward
            // Q2 = Right, Up, Back
            // Q3 = Right, Down, Forward
            // Q4 = Left, Up, Forward
            // Q5 = Right, Down, Back    = Q4
            // Q6 = Left, Up, Back       = Q3
            // Q7 = Left, Down, Forward  = Q2
            // Q8 = Left, Down, Back     = Q1

            /*
            coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.forward + Vector3.right, new Vector2(0.25f, 0.25f), 0f, 0f);// Q1|Q3 
            coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.back + Vector3.left, new Vector2(0.25f, 0.25f), 0f, 0f);    // Q1|Q3 (Q6|Q8)
            coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.back + Vector3.right, new Vector2(0.25f, 0.25f), 0f, 0f);   // Q2|Q4 (Q2|Q5)   Inside-out
            
            coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.down + Vector3.back, new Vector2(0.25f, 0.25f), 0f, 0f);    // Q1|Q4 (Q5|Q8)
            coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.down + Vector3.forward, new Vector2(0.25f, 0.25f), 0f, 0f); // Q2|Q3 (Q3|Q7)
            coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.up + Vector3.forward, new Vector2(0.25f, 0.25f), 0f, 0f);   // Q1|Q4
            */

            coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.up + Vector3.back, new Vector2(0.25f, 0.25f), 0f, 0f);      // Q2|Q3 (Q2|Q6)
            coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.forward + Vector3.left, new Vector2(0.25f, 0.25f), 0f, 0f); // Q2|Q4 (Q4|Q7)

            // coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.up + Vector3.left, new Vector2(0.25f, 0.25f), 0f, 0f);   // Q3|Q4 (Q4|Q6)
            // coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.up + Vector3.right, new Vector2(0.25f, 0.25f), 0f, 0f);  // Q1|Q2
            // coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.up + Vector3.back, new Vector2(0.25f, 0.25f), 0f, 0f);   // Q2|Q3 (Q2|Q6)
            // coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.down + Vector3.left, new Vector2(0.25f, 0.25f), 0f, 0f); // Q1|Q2 (Q7|Q8)
            // coordinates.drawBox(Vector3.zero, new Vector2(0.25f, 0.25f), Vector3.zero + Vector3.down + Vector3.right, new Vector2(0.25f, 0.25f), 0f, 0f);// Q3|Q4 (Q3|Q5)

            /*coordinates.drawBox(new Vector3(2, 1, 0), new Vector2(0.25f, 0.25f), new Vector3(2, 2, 0.5f), new Vector2(0.25f, 0.25f), 0f, 0f);
            coordinates.drawBox(new Vector3(2, 3, 0), new Vector2(0.25f, 0.25f), new Vector3(1, 3, 0.5f), new Vector2(0.25f, 0.25f), 0f, 0f);
            coordinates.drawBox(new Vector3(1, 1, 0), new Vector2(0.25f, 0.25f), new Vector3(0.5f, 0.5f, 0.5f), new Vector2(0.25f, 0.25f), 0f, 0f);*/
        }

        private static Shapes2D drawCoordinate(ref Vector2[] x, ref Vector2[] y, ref Vector2[] z)
        {
            var coordinates = new Shapes2D(new Vector2(400, 100));
            drawCoordinate(ref coordinates, ref x, 0.25f, 0.25f, 0.25f);
            drawCoordinate(ref coordinates, ref y, 2.75f, 0.25f, 0.25f);
            drawCoordinate(ref coordinates, ref z, 5.25f, 0.25f, 0.25f);
            return coordinates;
        }

        private static void drawCoordinate(ref Shapes2D coordinates, ref Vector2[] w, float xOffset, float yOffset, float width)
        {
            var multiplier = coordinates.size.y / 2.5f;
            if (w.Length > 0)
            {
                var x = (w[0].x + xOffset) * multiplier;
                var y = (w[0].y + yOffset) * multiplier;
                coordinates.drawCircle(new Vector2(x, y), Color.white, Color.white, width * (multiplier / 2f));
            }

            for (int i = 1; i < w.Length; i++)
            {
                var x1 = (w[i].x + xOffset) * multiplier;
                var y1 = (w[i].y + yOffset) * multiplier;
                var x2 = (w[i-1].x + xOffset) * multiplier;
                var y2 = (w[i-1].y + yOffset) * multiplier;
                coordinates.drawCircle(new Vector2(x1, y1), Color.white, Color.white, width * (multiplier / 2f));
                coordinates.drawRectangle(new Vector2(x1, y1), new Vector2(x2, y2), Color.white, Color.white, width * multiplier);
            }
        }

        private static Vector3[] getCoordinate3D(int[] coordinate)
        {
            var vectors = new Vector3[coordinate.Length];
            for (int i = 0; i < coordinate.Length; i++)
            {
                var vector = getCoordinate2D(coordinate[i]);
                vectors[i] = new Vector3(-1 * vector.x, 0f, -1 * vector.y);
            }
            return vectors;
        }

        private static Vector2[] getCoordinate2D(int[] coordinate)
        {
            var vectors = new Vector2[coordinate.Length];
            for (int i = 0; i < coordinate.Length; i++)
            {
                vectors[i] = getCoordinate2D(coordinate[i]);
            }
            return vectors;
        }

        private static Vector2 getCoordinate2D(int coordinate)
        {
            if (coordinate == 0)
            {
                return new Vector2(0.5f, 1.732f);
            }
            if (coordinate == 1)
            {
                return new Vector2(1.5f, 1.732f);
            }
            if (coordinate == 2)
            {
                return new Vector2(2f, 0.866f);
            }
            if (coordinate == 3)
            {
                return new Vector2(1.5f, 0f);
            }
            if (coordinate == 4)
            {
                return new Vector2(0.5f, 0f);
            }
            if (coordinate == 5)
            {
                return new Vector2(0f, 0.866f);
            }
            return new Vector2(1f, 0.866f);
        }

        private static int[] generateCoordinate(ref System.Random random)
        {
            var coodinate = random.Next(0, 63);
            var list = new List<int>();
            if ((coodinate & 0x1) != 0)
            {
                list.Add(1);
            }
            if ((coodinate & 0x2) != 0)
            {
                list.Add(2);
            }
            if ((coodinate & 0x4) != 0)
            {
                list.Add(3);
            }
            if ((coodinate & 0x8) != 0)
            {
                list.Add(4);
            }
            if ((coodinate & 0x10) != 0)
            {
                list.Add(5);
            }
            if ((coodinate & 0x20) != 0)
            {
                list.Add(0);
            }
            Shuffle(list, ref random);

            return list.ToArray();
        }

        private static void Shuffle<T>(this IList<T> list, ref System.Random random)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}

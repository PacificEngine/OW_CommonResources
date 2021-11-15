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
                coordinates.drawSphere(new Vector3(x, y, z), width * (multiplier / 2f), 8);
            }

            for (int i = 1; i < w.Length; i++)
            {
                var x1 = (w[i].x + xOffset) * multiplier;
                var y1 = (w[i].y) * multiplier;
                var z1 = (w[i].z) * multiplier;
                var x2 = (w[i - 1].x + xOffset) * multiplier;
                var y2 = (w[i - 1].y) * multiplier;
                var z2 = (w[i - 1].z) * multiplier;
                coordinates.drawSphere(new Vector3(x1, y1, z1), width * (multiplier / 2f), 8);
                coordinates.drawCylinder(new Vector3(x1, y1, z1), new Vector3(x2, y2, z2), width * (multiplier / 2f), width * (multiplier / 2f), 25);
            }
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
    }
}

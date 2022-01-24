using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game.Player;
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
        private static EntryData? shipEntry = null;
        private static ShipLogEntry shipLogEntry = null;
        private static System.Random random = new System.Random();

        public static bool enabledManagement { get; set; } = false;

        private static bool requireUpdate = false;
        private static Tuple<int[], int[], int[]> _defaultCoordinates = standardCoordinates;
        private static Tuple<int[], int[], int[]> _coordinates = standardCoordinates;
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

        public static Tuple<int[], int[], int[]> standardCoordinates
        {
            get
            {
                return Tuple.Create(new int[] { 1, 5, 4 }, new int[] { 3, 0, 1, 4 }, new int[] { 1, 2, 3, 0, 5, 4 });
            }
        }

        public static Tuple<int[], int[], int[]> defaultMapping
        {
            get
            {
                return Tuple.Create(_defaultCoordinates.Item1, _defaultCoordinates.Item2, _defaultCoordinates.Item3);
            }
            set
            {
                _defaultCoordinates = Tuple.Create(value.Item1, value.Item2, value.Item3);
                enabledManagement = true;
            }
        }

        public static Tuple<int[], int[], int[]> coordinates
        {
            get
            {
                return Tuple.Create(x, y, z);
            }
            set
            {
                _coordinates = value;
                enabledManagement = true;
                requireUpdate = true;
            }
        }

        public static Tuple<int[], int[], int[]> defaultCoordinates
        {
            get
            {
                return Tuple.Create(new int[] { 1, 5, 4 }, new int[] { 3, 0, 1, 4 }, new int[] { 1, 2, 3, 0, 5, 4 });
            }
        }


        public static void Start()
        {
            Helper.helper.HarmonyHelper.AddPrefix<NomaiCoordinateInterface>("Awake", typeof(EyeCoordinates), "onNomaiCoordinateInterfaceAwake");
            Helper.helper.HarmonyHelper.AddPostfix<KeyInfoPromptController>("Start", typeof(EyeCoordinates), "onKeyInfoPromptControllerStart");
            Helper.helper.HarmonyHelper.AddPostfix<OrbitalCannonHologramProjector>("Awake", typeof(EyeCoordinates), "onOrbitalCannonHologramProjectorAwake");
        }

        public static void Awake()
        {
        }

        public static void Destroy()
        {
        }

        public static void Update()
        {
            var card = Data.getFactEntry("OPC_SUNKEN_MODULE");
            if (!shipEntry.HasValue && card != null && card.Item1.HasValue && card.Item2 != null)
            {
                shipEntry = card.Item1;
                shipLogEntry = card.Item2;
                requireUpdate = true;
            }

            updateCoordinates();
        }

        public static void FixedUpdate()
        {
        }

        public static Shapes2D getCoordinatesImage()
        {
            var x = getCoordinate2D(EyeCoordinates.x);
            var y = getCoordinate2D(EyeCoordinates.y);
            var z = getCoordinate2D(EyeCoordinates.z);
            return drawCoordinate2D(ref x, ref y, ref z);
        }

        public static Shapes3D getCoordinatesModel()
        {
            var x = getCoordinate3D(EyeCoordinates.x);
            var y = getCoordinate3D(EyeCoordinates.y);
            var z = getCoordinate3D(EyeCoordinates.z);
            return drawCoordinate3D(ref x, ref y, ref z);
        }

        private static void updateCoordinates()
        {
            if (enabledManagement && requireUpdate)
            {
                requireUpdate = false;

                x = _coordinates.Item1;
                y = _coordinates.Item2;
                z = _coordinates.Item3;

                var texture = getCoordinatesImage().getTexture();
                if (keyInfoPromptController)
                {
                    var manager = Locator.GetPromptManager();
                    var oldPrompt = keyInfoPromptController.GetValue<ScreenPrompt>("_eyeCoordinatesPrompt");
                    manager.RemoveScreenPrompt(oldPrompt);
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

                var oldCard = Data.getFactEntry("OPC_SUNKEN_MODULE");
                if (oldCard != null && oldCard.Item1.HasValue && oldCard.Item2 != null)
                {
                    var newEntry = new EntryData();
                    newEntry.id = oldCard.Item1.Value.id;
                    newEntry.sprite = createSprite(texture, oldCard.Item1.Value.sprite);
                    newEntry.altSprite = createSprite(texture, oldCard.Item1.Value.altSprite);
                    newEntry.cardPosition = oldCard.Item1.Value.cardPosition;


                    oldCard.Item2.SetSprite(newEntry.sprite);
                    oldCard.Item2.SetAltSprite(newEntry.altSprite);

                    Data.putFactEntry(newEntry, oldCard.Item2);
                }
            }
        }

        private static Sprite createSprite(Texture2D oldTexture, Sprite oldSprite)
        {
            var width = oldSprite.rect.width;
            var height = oldSprite.rect.height;
            var ratio = width / height;

            if (oldTexture.width > oldTexture.height)
            {
                width = oldTexture.width;
                height = oldTexture.height * ratio;
            }
            else
            {

                width = oldTexture.width / ratio;
                height = oldTexture.height;
            }

            var shape = new Shapes2D(new Vector2(width, height));

            var x = ((width - oldTexture.width) / 2f);
            var y = ((height - oldTexture.height) / 2f);
            shape.drawTexture(oldTexture, new Vector2(x, y));
            var texture = shape.getTexture();
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        private static bool onNomaiCoordinateInterfaceAwake(ref NomaiCoordinateInterface __instance)
        {
            EyeCoordinates.nomaiCoordinateInterface = __instance;
            coordinates = Tuple.Create(_x, _y, _z);
            return true;
        }

        private static void onKeyInfoPromptControllerStart(ref KeyInfoPromptController __instance)
        {
            keyInfoPromptController = __instance;
            requireUpdate = true;
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
            requireUpdate = true;
        }

        private static Shapes3D drawCoordinate3D(ref Vector3[] x, ref Vector3[] y, ref Vector3[] z)
        {
            var coordinates = new Shapes3D();
            drawCoordinate3D(ref coordinates, ref x, 1.25f, 0.5f, 0.25f);
            drawCoordinate3D(ref coordinates, ref y, -1.25f, 0.5f, 0.25f);
            drawCoordinate3D(ref coordinates, ref z, -3.75f, 0.5f, 0.25f);
            return coordinates;
        }

        private static void drawCoordinate3D(ref Shapes3D coordinates, ref Vector3[] w, float xOffset, float height, float width)
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

        private static Shapes2D drawCoordinate2D(ref Vector2[] x, ref Vector2[] y, ref Vector2[] z)
        {
            var width = 1024f;
            var height = (width / 7.5f) * 2.5f;
            var coordinates = new Shapes2D(new Vector2(width, height));
            drawCoordinate2D(ref coordinates, ref x, 0.25f, 0.25f, 0.25f);
            drawCoordinate2D(ref coordinates, ref y, 2.75f, 0.25f, 0.25f);
            drawCoordinate2D(ref coordinates, ref z, 5.25f, 0.25f, 0.25f);
            return coordinates;
        }

        private static void drawCoordinate2D(ref Shapes2D coordinates, ref Vector2[] w, float xOffset, float yOffset, float width)
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

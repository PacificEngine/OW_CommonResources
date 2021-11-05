using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PacificEngine.OW_CommonResources
{
    public static class EyeCoordinates
    {
        private static KeyInfoPromptController keyInfoPromptController = null;
        private static NomaiCoordinateInterface nomaiCoordinateInterface = null;
        private static ScreenPromptElement eyePromptElement = null;
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

        public static void randomizeCoordinates()
        {
            setCoordinates(generateCoordinate(), generateCoordinate(), generateCoordinate());
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
                var texture = drawCoordinate(getCoordinate(x), getCoordinate(y), getCoordinate(z));
                var eyePrompt = new ScreenPrompt(UITextLibrary.GetString(UITextType.EyeCoordinates) + "<EYE>", Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero));
                eyePromptElement = manager.AddScreenPrompt(eyePrompt, manager.GetScreenPromptList(PromptPosition.LowerLeft), manager.GetTextAnchor(PromptPosition.LowerLeft));
                keyInfoPromptController.SetValue("_eyeCoordinatesPrompt", eyePrompt);
            }
        }

        public static Texture2D getCoordinatesImage()
        {
            return drawCoordinate(getCoordinate(x), getCoordinate(y), getCoordinate(z));
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

        private static Texture2D drawCoordinate(Vector2[] x, Vector2[] y, Vector2[] z)
        {
            var coordinates = new Shapes(400, 100);
            drawCoordinate(coordinates, x, 0.5f);
            drawCoordinate(coordinates, y, 3.5f);
            drawCoordinate(coordinates, z, 6.5f);
            return coordinates.getTexture();
        }

        private static void drawCoordinate(Shapes coordinates, Vector2[] w, float xOffset)
        {
            var multiplier = coordinates.height / 2.5f;
            var yOffset = 0.386f;
            var width = 0.25f;
            if (w.Length > 0)
            {
                var x = (w[0].x + xOffset) * multiplier;
                var y = (w[0].y + yOffset) * multiplier;
                coordinates.addDot((int)x, (int)y, Color.white, Color.white, width * (multiplier / 2f));
            }

            for (int i = 1; i < w.Length; i++)
            {
                var x1 = (w[i].x + xOffset) * multiplier;
                var y1 = (w[i].y + yOffset) * multiplier;
                var x2 = (w[i-1].x + xOffset) * multiplier;
                var y2 = (w[i-1].y + yOffset) * multiplier;
                coordinates.addDot((int)x1, (int)y1, Color.white, Color.white, width * (multiplier / 2f));
                coordinates.drawLine((int)x1, (int)y1, (int)x2, (int)y2, Color.white, Color.white, width * multiplier);
            }
        }

        private static Vector2[] getCoordinate(int[] coordinate)
        {
            var vectors = new Vector2[coordinate.Length];
            for (int i = 0; i < coordinate.Length; i++)
            {
                vectors[i] = getCoordinate(coordinate[i]);
            }
            return vectors;
        }

        private static Vector2 getCoordinate(int coordinate)
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

        private static int[] generateCoordinate()
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
            Shuffle(list);

            return list.ToArray();
        }

        private static void Shuffle<T>(this IList<T> list)
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

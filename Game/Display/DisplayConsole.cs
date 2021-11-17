using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Game.Display
{
    public enum ConsoleLocation
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public class DisplayConsole
    {
        private static Dictionary<ConsoleLocation, DisplayConsole> _locations = new Dictionary<ConsoleLocation, DisplayConsole>();

        public static DisplayConsole getConsole(ConsoleLocation location)
        {
            DisplayConsole console;
            if (_locations.TryGetValue(location, out console))
            {
                return console;
            }
            console = new DisplayConsole(location);
            _locations.Add(location, console);
            return console;
        }

        public static void OnGUI()
        {
            foreach(var console in _locations.Values)
            {
                console.onGUI();
            }
        }

        private static float heightPerElement = 20f;
        private static float width = 300f;
        private static Dictionary<string, Tuple<float, string>> _elements = new Dictionary<string, Tuple<float, string>>();
        private ConsoleLocation _location;

        private DisplayConsole(ConsoleLocation location)
        {
            _location = location;
        }

        public void setElement(string id, string value, float priority)
        {
            if (value.Length == 0)
            {
                _elements.Remove(id);
            }
            else
            {
                _elements[id] = Tuple.Create(priority, value);
            }
        }

        public void onGUI()
        {
            var elements = new List<Tuple<float, string>>(_elements.Values);
            if (_location == ConsoleLocation.TopLeft)
            {
                float i = 0f;
                elements.Sort((x1, x2) => x1.Item1.CompareTo(x2.Item1));
                foreach (var element in elements)
                {
                    GUI.Label(new Rect(width, (heightPerElement * i++), width, heightPerElement), element.Item2);
                }
            }
            else if (_location == ConsoleLocation.TopRight)
            {
                float i = 0f;
                elements.Sort((x1, x2) => x1.Item1.CompareTo(x2.Item1));
                foreach (var element in elements)
                {
                    GUI.Label(new Rect(((float)Screen.width) - width, (heightPerElement * i++), width, heightPerElement), element.Item2);
                }
            }
            else if (_location == ConsoleLocation.BottomLeft)
            {
                float i = 1f;
                elements.Sort((x1, x2) => -1 * x1.Item1.CompareTo(x2.Item1));
                foreach (var element in elements)
                {
                    GUI.Label(new Rect(width, ((float)Screen.height) - (heightPerElement * i++), width, heightPerElement), element.Item2);
                }
            }
            else if (_location == ConsoleLocation.BottomRight)
            {
                float i = 1f;
                elements.Sort((x1, x2) => -1 * x1.Item1.CompareTo(x2.Item1));
                foreach (var element in elements)
                {
                    GUI.Label(new Rect(((float)Screen.width) - width, ((float)Screen.height) - (heightPerElement * i++), width, heightPerElement), element.Item2);
                }
            }
        }
    }
}

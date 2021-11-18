using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using PacificEngine.OW_CommonResources.Game;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PacificEngine.OW_CommonResources.Config
{
    public class InputClass
    {
        private const char split = ',';
        private HashSet<Key> keys;
        private int pressed = 0;

        private InputClass(HashSet<Key> keys)
        {
            this.keys = keys;
        }

        public InputClass(params Key[] keys)
        {
            this.keys = new HashSet<Key>(keys);
        }

        public void Update()
        {
            bool areAllPressed = true;
            foreach (Key key in keys)
            {
                if (!Keyboard.current[key].IsActuated())
                {
                    areAllPressed = false;
                    break;
                }
            }

            if (areAllPressed)
            {
                pressed++;
            }
            else
            {
                pressed = 0;
            }
        }

        public List<Key> getKeys()
        {
            return new List<Key>(keys);
        }

        public static InputClass fromString(string keysString)
        {
            HashSet<Key> keys = new HashSet<Key>();
            foreach (string keyString in keysString.Split(split))
            {
                try
                {
                    var key = (Key)Enum.Parse(Key.A.GetType(), keyString, true);
                    int keyInt;
                    if (int.TryParse(keyString, out keyInt))
                    {
                        Helper.helper.Console.WriteLine("Key `" + keyString + "` is not recgonized.", MessageType.Warning);
                    }
                    keys.Add(key);
                } catch (Exception e)
                {
                    Helper.helper.Console.WriteLine("Key `" + keyString + "` is not recgonized.", MessageType.Warning);
                }
            }
            return new InputClass(keys);
        }

        public override string ToString()
        {
            string value = "";
            foreach (Key key in keys)
            {
                value += Enum.GetName(key.GetType(), key);
                value += split;
            }
            return value.Substring(0, value.Length - 1);
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is InputClass)
            {
                var obj = other as InputClass;
                if (obj.keys.Count == keys.Count)
                {
                    foreach (var key in keys)
                    {
                        if (!obj.keys.Contains(key))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var key in keys)
            {
                hash += ((int)key) * 4;
            }
            return hash;
        }

        public bool isPressedThisFrame()
        {
            return pressed == 1;
        }

        public bool isPressed()
        {
            return pressed > 0;
        }

        public int frameCountPressed()
        {
            return pressed;
        }
    }

    public class MultiInputClass
    {
        private const char split = '|';
        HashSet<InputClass> keys;
        private int pressed = 0;

        private MultiInputClass(HashSet<InputClass> keys)
        {
            this.keys = keys;
        }

        public MultiInputClass(params InputClass[] keys)
        {
            this.keys = new HashSet<InputClass>(keys);
        }

        public void Update()
        {
            bool isPressed = false;
            foreach (InputClass key in keys)
            {
                key.Update();
                if (key.isPressed())
                {
                    isPressed = true;
                }
            }
            if (isPressed)
            {
                pressed++;
            }
            else
            {
                pressed = 0;
            }
        }

        public List<InputClass> getKeysCombos()
        {
            return new List<InputClass>(keys);
        }

        public static MultiInputClass fromString(string keysString)
        {
            HashSet<InputClass> keys = new HashSet<InputClass>();
            foreach (string keyString in keysString.Split(split))
            {
                keys.Add(InputClass.fromString(keyString));
            }
            return new MultiInputClass(keys);
        }

        public override string ToString()
        {
            string value = "";
            foreach (InputClass key in keys)
            {
                value += key.ToString();
                value += split;
            }
            return value.Substring(0, value.Length - 1);
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is MultiInputClass)
            {
                var obj = other as MultiInputClass;
                if (obj.keys.Count == keys.Count)
                {
                    foreach (var key in keys)
                    {
                        if (!obj.keys.Contains(key))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var key in keys)
            {
                hash += key.GetHashCode() * 16;
            }
            return hash;
        }

        public bool isPressedThisFrame()
        {
            return pressed == 1;
        }

        public bool isPressed()
        {
            return pressed > 0;
        }

        public int frameCountPressed()
        {
            return pressed;
        }
    }
}

using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PacificEngine.OW_CommonResources.Game.Config
{
    public class InputClass
    {
        private const char _split = ',';
        private HashSet<Key> _keys;
        private int _pressed = 0;
        private float _time = 0f;

        private InputClass(HashSet<Key> keys)
        {
            this._keys = keys;
        }

        public InputClass(params Key[] keys)
        {
            this._keys = new HashSet<Key>(keys);
        }

        public void Update()
        {
            bool areAllPressed = true;
            foreach (Key key in _keys)
            {
                if (!Keyboard.current[key].IsActuated())
                {
                    areAllPressed = false;
                    break;
                }
            }

            if (areAllPressed)
            {
                _pressed++;
            }
            else
            {
                _pressed = 0;
            }
        }

        public List<Key> getKeys()
        {
            return new List<Key>(_keys);
        }

        public static InputClass fromString(string keysString)
        {
            HashSet<Key> keys = new HashSet<Key>();
            foreach (string keyString in keysString.Split(_split))
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
                } catch (Exception)
                {
                    Helper.helper.Console.WriteLine("Key `" + keyString + "` is not recgonized.", MessageType.Warning);
                }
            }
            return new InputClass(keys);
        }

        public override string ToString()
        {
            string value = "";
            foreach (Key key in _keys)
            {
                value += Enum.GetName(key.GetType(), key);
                value += _split;
            }
            return value.Substring(0, value.Length - 1);
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is InputClass)
            {
                var obj = other as InputClass;
                if (obj._keys.Count == _keys.Count)
                {
                    foreach (var key in _keys)
                    {
                        if (!obj._keys.Contains(key))
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
            foreach (var key in _keys)
            {
                hash += ((int)key) * 4;
            }
            return hash;
        }

        public bool isPressedThisFrame()
        {
            return _pressed == 1;
        }

        public bool isPressed()
        {
            return _pressed > 0;
        }

        public int frameCountPressed()
        {
            return _pressed;
        }

        public float timePressed()
        {
            return (_time == 0f) ? 0f : (Time.time - _time);
        }

        public int count()
        {
            return _keys.Count;
        }
    }

    public class MultiInputClass
    {
        private const char _split = '|';
        HashSet<InputClass> _keys;
        private int _pressed = 0;
        private float _time = 0f;
        private int _keyMatchCount = 0;

        private MultiInputClass(HashSet<InputClass> keys)
        {
            _keys = keys;
        }

        public MultiInputClass(params InputClass[] keys)
        {
            _keys = new HashSet<InputClass>(keys);
        }

        public void Update()
        {
            bool isPressed = false;
            foreach (InputClass key in _keys)
            {
                key.Update();
                if (key.isPressed())
                {
                    isPressed = true;
                    if (key.count() > _keyMatchCount)
                    {
                        _keyMatchCount = key.count();
                    }
                }
            }
            if (isPressed)
            {
                _pressed++;
                if (_time == 0f)
                {
                    _time = Time.time;
                }
            }
            else
            {
                _pressed = 0;
                _time = 0f;
                _keyMatchCount = 0;
            }
        }

        public List<InputClass> getKeysCombos()
        {
            return new List<InputClass>(_keys);
        }

        public static MultiInputClass fromString(string keysString)
        {
            HashSet<InputClass> keys = new HashSet<InputClass>();
            foreach (string keyString in keysString.Split(_split))
            {
                keys.Add(InputClass.fromString(keyString));
            }
            return new MultiInputClass(keys);
        }

        public override string ToString()
        {
            string value = "";
            foreach (InputClass key in _keys)
            {
                value += key.ToString();
                value += _split;
            }
            return value.Substring(0, value.Length - 1);
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is MultiInputClass)
            {
                var obj = other as MultiInputClass;
                if (obj._keys.Count == _keys.Count)
                {
                    foreach (var key in _keys)
                    {
                        if (!obj._keys.Contains(key))
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
            foreach (var key in _keys)
            {
                hash += key.GetHashCode() * 16;
            }
            return hash;
        }

        public bool isPressedThisFrame()
        {
            return _pressed == 1;
        }

        public bool isPressed()
        {
            return _pressed > 0;
        }

        public int frameCountPressed()
        {
            return _pressed;
        }

        public float timePressed()
        {
            return (_time == 0f) ? 0f : (Time.time - _time);
        }

        public int keyMatchCount()
        {
            return _keyMatchCount;
        }
    }
}

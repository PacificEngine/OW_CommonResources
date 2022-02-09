using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Game.Resource
{
    public class HeavenlyBody : IEquatable<HeavenlyBody>
    {
        private static Dictionary<string, HeavenlyBody> _map = new Dictionary<string, HeavenlyBody>();
        private static int _nextValue = 1;

        public readonly static HeavenlyBody None = new HeavenlyBody("None", 0, true);

        private int _value;
        private string _name;
        private bool _pseudoHeavenlyBody;

        public int value { get { return _value; } }
        public string name { get { return _name; } }
        public bool pseudoHeavenlyBody { get { return _pseudoHeavenlyBody; } }

        public static HeavenlyBody FromString(string name, bool create = false)
        {
            HeavenlyBody value;
            if (_map.TryGetValue(name, out value))
            {
                return value;
            }
            else if (create)
            {
                return new HeavenlyBody(name);
            }
            else
            {
                return HeavenlyBody.None;
            }
        }

        public static HeavenlyBody[] GetValues()
        {
            return _map.Values.ToArray();
        }

        private HeavenlyBody(string name, int value, bool isPseudoHeavenlyBody = false)
        {
            this._value = value;
            this._name = name;
            this._pseudoHeavenlyBody = isPseudoHeavenlyBody;

            _map.Add(_name, this);
        }

        public HeavenlyBody(string name, bool isPseudoHeavenlyBody = false)
        {
            if (name == null)
            {
                throw new ArgumentException($"name cannot be null");
            }
            else if (name.Length == 0)
            {
                throw new ArgumentException($"name cannot be blank");
            }
            else if (_map.ContainsKey(name))
            {
                throw new ArgumentException($"{name} already in use");
            }

            this._value = _nextValue++;
            this._name = name;
            this._pseudoHeavenlyBody = isPseudoHeavenlyBody;

            _map.Add(_name, this);
        }

        public override string ToString()
        {
            return _name;
        }

        public override bool Equals(System.Object other)
        {
            if (other is null && this._value == None._value)
            {
                return true;
            }
            else if (!(other is null) && other is HeavenlyBody)
            {
                return Equals((HeavenlyBody)(other as HeavenlyBody));
            }
            return false;
        }

        public bool Equals(HeavenlyBody other)
        {
            if (other is null && this._value == None._value)
            {
                return true;
            }
            if (other != null)
            {
                return _value == other._value;
            }
            return false;
        }

        public static bool operator ==(HeavenlyBody x, HeavenlyBody y)
        {
            if (x is null)
            {
                return y is null || y._value == None._value;
            }
            else if (y is null)
            {
                return x is null || x._value == None._value;
            }
            else
            {
                return x._value == y._value;
            }
        }

        public static bool operator !=(HeavenlyBody x, HeavenlyBody y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            return _value;
        }

        ~HeavenlyBody()
        {
            if (_name != null)
            {
                _map.Remove(_name);
            }
            _value = None._value;
            _name = null;
        }
    }
}

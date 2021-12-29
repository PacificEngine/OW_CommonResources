using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Game.Resource
{
    public class HeavenlyBody
    {
        private static Dictionary<string, HeavenlyBody> _map = new Dictionary<string, HeavenlyBody>();
        private static int _nextValue = 0;

        public readonly static HeavenlyBody None = new HeavenlyBody("None");

        private int _value;
        private string _name;

        public int value { get { return _value; } }
        public string name { get { return name; } }

        public static HeavenlyBody FromString(string name)
        {
            HeavenlyBody value;
            if (_map.TryGetValue(name, out value))
            {
                return value;
            }

            return HeavenlyBody.None;
        }

        public static HeavenlyBody[] GetValues()
        {
            return _map.Values.ToArray();
        }

        public HeavenlyBody(string name)
        {
            if (_map.ContainsKey(name))
            {
                throw new ArgumentException($"{name} already in use");
            }

            this._value = _nextValue++;
            this._name = name;

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
                var obj = other as HeavenlyBody;
                return _value == obj._value;
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

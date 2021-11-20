using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.InputSystem;

namespace PacificEngine.OW_CommonResources.Game.Config
{
    public class InputMapping<T>
    {
        private Dictionary<T, MultiInputClass> _inputMap = new Dictionary<T, MultiInputClass>();

        public void addInput(string input, T value)
        {
            addInput(getInputConfig(input), value);
        }

        public void addInput(IModConfig config, T option, string defaultValue)
        {
            var name = Enum.GetName(option.GetType(), option).Replace("_", " ");
            var input = getInputConfigOrDefault(config, name, defaultValue);
            addInput(input, option);
        }

        private void addInput(MultiInputClass input, T value)
        {
            _inputMap.Add(value, input);
        }

        private MultiInputClass getInputConfig(string input)
        {
            return MultiInputClass.fromString(input);
        }

        private MultiInputClass getInputConfigOrDefault(IModConfig config, string id, string defaultValue)
        {
            return getInputConfig(ConfigHelper.getConfigOrDefault<string>(config, id, defaultValue));
        }

        public void Clear()
        {
            _inputMap.Clear();
        }

        public void Update()
        {
            foreach (MultiInputClass input in _inputMap.Values)
            {
                input.Update();
            }
        }

        public List<Tuple<T, MultiInputClass>> getPressedThisFrame()
        {
            var bindings = new List<Tuple<T, MultiInputClass>>();
            foreach (var keyBindings in _inputMap)
            {
                if (keyBindings.Value.isPressedThisFrame())
                {
                    bindings.Add(Tuple.Create(keyBindings.Key, keyBindings.Value));
                }
            }
            bindings.Sort((x1, x2) => x2.Item2.keyMatchCount().CompareTo(x1.Item2.keyMatchCount()));
            return bindings;
        }

        public List<Tuple<T, MultiInputClass>> getPressed()
        {
            var bindings = new List<Tuple<T, MultiInputClass>>();
            foreach (var keyBindings in _inputMap)
            {
                if (keyBindings.Value.isPressed())
                {
                    bindings.Add(Tuple.Create(keyBindings.Key, keyBindings.Value));
                }
            }
            bindings.Sort((x1, x2) => x2.Item2.keyMatchCount().CompareTo(x1.Item2.keyMatchCount()));
            return bindings;
        }

        public HashSet<Key> getKeysPressed()
        {
            var keys = new HashSet<Key>();
            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (Keyboard.current[key].IsActuated())
                {
                    keys.Add(key);
                }
            }

            return keys;
        }
    }
}

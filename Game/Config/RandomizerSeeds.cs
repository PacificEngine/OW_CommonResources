using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Game.Config
{
    public class RandomizerSeeds
    {
        public enum Type
        {
            None,
            Seed,
            Profile,
            Death,
            Minute,
            Use,
            MinuteUse,
            FullUse,
            Seedless,
            SeedlessMinute,
            SeedlessUse,
            SeedlessMinuteUse,
            SeedlessFullUse
        }

        private int _seed;
        private int _normalSeed;
        private int _profileSeed;
        private int _deathSeed;
        private int _minuteSeed;
        private int _useSeed;
        private int _minuteUseSeed;
        private int _fullUseSeed;
        public int seed { get { return _seed; } set { _seed = value; reset(); } }
        public Type type { get; set; }
        private Random _defaultRandom;
        private Dictionary<Type, Random> _random = new Dictionary<Type, Random>();

        private SeedIncrement _minuteSeedGet;
        private SeedIncrement _useSeedGet;
        private SeedIncrement _fullUseSeedGet;

        private int _lastMinuteSeedIncrement;
        private int _lastUseSeedIncrement;
        private int _lastFullUseSeedIncrement;


        public delegate int SeedIncrement();
        private delegate T getRandom<T>(Random random);

        public RandomizerSeeds(int seed, Type type, SeedIncrement minute, SeedIncrement use, SeedIncrement fullUse)
        {
            _minuteSeedGet = minute;
            _useSeedGet = use;
            _fullUseSeedGet = fullUse;
            reset(seed, type);
        }

        public void Awake()
        {
            reset();
        }

        public void reset(int seed, Type type)
        {
            this._seed = seed;
            this.type = type;
            reset();
        }

        public void reset()
        {
            _random.Clear();

            var seeds = new Random(_seed);
            _normalSeed = seeds.Next();
            _profileSeed = seeds.Next();
            _deathSeed = seeds.Next();
            _minuteSeed = seeds.Next();
            _useSeed = seeds.Next();
            _minuteUseSeed = seeds.Next();
            _fullUseSeed = seeds.Next();

            _defaultRandom = new Random(_seed);

            _lastMinuteSeedIncrement = 0;
            _lastUseSeedIncrement = 0;
            _lastFullUseSeedIncrement = 0;
        }

        private T getValue<T>(Type type, getRandom<T> random)
        {
            var currentMinuteSeedIncrement = _minuteSeedGet.Invoke();
            var currentUseSeedIncrement = _useSeedGet.Invoke();
            var currentFullUseSeedIncrement = _fullUseSeedGet.Invoke();

            if (currentMinuteSeedIncrement != _lastMinuteSeedIncrement)
            {
                _random.Remove(Type.Minute);
                _random.Remove(Type.SeedlessMinute);
                _random.Remove(Type.MinuteUse);
                _random.Remove(Type.SeedlessMinuteUse);
            }
            if (currentUseSeedIncrement != _lastUseSeedIncrement)
            {
                _random.Remove(Type.Use);
                _random.Remove(Type.SeedlessUse);
                _random.Remove(Type.MinuteUse);
                _random.Remove(Type.SeedlessMinuteUse);
            }
            if (currentFullUseSeedIncrement != _lastFullUseSeedIncrement)
            {
                _random.Remove(Type.FullUse);
                _random.Remove(Type.SeedlessFullUse);
            }

            _lastMinuteSeedIncrement = currentMinuteSeedIncrement;
            _lastUseSeedIncrement = currentUseSeedIncrement;
            _lastFullUseSeedIncrement = currentFullUseSeedIncrement;

            Random rand;
            if (_random.TryGetValue(type, out rand))
            {
                return random.Invoke(rand);
            }

            switch (type)
            {
                case Type.Seed:
                    _random.Add(type, new Random(_normalSeed));
                    break;
                case Type.Profile:
                    var profileName = StandaloneProfileManager.SharedInstance?.currentProfile?.profileName;
                    if (profileName != null)
                    {
                        _random.Add(type, new Random(_profileSeed + profileName.GetHashCode()));
                    }
                    break;
                case Type.Death:
                    var loopCount = StandaloneProfileManager.SharedInstance?.currentProfileGameSave?.fullTimeloops;
                    if (loopCount.HasValue)
                    {
                        _random.Add(type, new Random(_profileSeed + loopCount.Value.GetHashCode()));
                    }
                    break;
                case Type.Minute:
                    _random.Add(type, new Random(_minuteSeed + _lastMinuteSeedIncrement));
                    break;
                case Type.Use:
                    _random.Add(type, new Random(_useSeed + _lastUseSeedIncrement));
                    break;
                case Type.MinuteUse:
                    var randomSeeder = new Random(_minuteUseSeed + _lastMinuteSeedIncrement);
                    _random.Add(type, new Random(randomSeeder.Next() + _lastUseSeedIncrement));
                    break;
                case Type.FullUse:
                    _random.Add(type, new Random(_fullUseSeed + _lastFullUseSeedIncrement));
                    break;
                case Type.Seedless:
                case Type.SeedlessMinute:
                case Type.SeedlessUse:
                case Type.SeedlessMinuteUse:
                case Type.SeedlessFullUse:
                    _random.Add(type, new Random());
                    break;
                default:
                    Helper.helper.Console.WriteLine("Random Type `" + type + "` is not programmed for Seeding.", MessageType.Warning);
                    break;
            }

            if (_random.TryGetValue(type, out rand))
            {
                return random.Invoke(rand);
            }
            return random.Invoke(_defaultRandom);
        }

        public int Next()
        {
            return Next(type);
        }

        public int Next(int minValue, int maxValue)
        {
            return Next(type, minValue, maxValue);
        }

        public int Next(int maxValue)
        {
            return Next(type, maxValue);
        }

        public void NextBytes(byte[] buffer)
        {
            NextBytes(type, buffer);
        }

        public double NextDouble()
        {
            return NextDouble(type);
        }

        public double NextRange(double min, double max)
        {
            return NextRange(type, min, max);
        }

        public int Next(Type type)
        {
            return getValue(type, (random) => random.Next());
        }

        public int Next(Type type, int minValue, int maxValue)
        {
            return getValue(type, (random) => random.Next(minValue, maxValue));
        }

        public int Next(Type type, int maxValue)
        {
            return getValue(type, (random) => random.Next(maxValue));
        }

        public void NextBytes(Type type, byte[] buffer)
        {
            getValue(type, (random) => { random.NextBytes(buffer); return 0; });
        }

        public double NextDouble(Type type)
        {
            return getValue(type, (random) => random.NextDouble());
        }

        public double NextRange(Type type, double min, double max)
        {
            return (NextDouble(type) * (max - min)) + min;
        }
    }
}

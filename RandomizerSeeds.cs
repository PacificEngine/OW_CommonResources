using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources
{
    public class RandomizerSeeds
    {
        public enum Type
        {
            Seed,
            Profile,
            Death,
            Minute,
            Use,
            Seedless,
            SeedlessMinute,
            SeedlessUse
        }

        private int _seed;
        private int _profileSeed;
        private int _deathSeed;
        public int seed { get { return _seed; } set { _seed = value; reset(); } }
        public Type type { get; set; }
        private Random _seedRandom;
        private Random _profileRandom;
        private Random _deathRandom;
        private Random _minuteRandom;
        private Random _useRandom;
        private Random _seedlessRandom;

        private delegate T getRandom<T>(Random random);

        public RandomizerSeeds(int seed, Type type)
        {
            this._seed = seed;
            this.type = type;
            reset();
        }

        public void Awake()
        {
            reset();
        }

        public void reset()
        {
            _profileRandom = null;
            _deathRandom = null;
            _seedlessRandom = new Random();

            _seedRandom = new Random(_seed);
            _minuteRandom = new Random(_seedRandom.Next());
            _useRandom = new Random(_seedRandom.Next());
            _profileSeed = _seedRandom.Next();
            _deathSeed = _seedRandom.Next();

            var profileName = StandaloneProfileManager.SharedInstance?.currentProfile?.profileName;
            var loopCount = StandaloneProfileManager.SharedInstance?.currentProfileGameSave?.fullTimeloops;


            if (profileName != null)
            {
                _profileRandom = new Random(_profileSeed + profileName.GetHashCode());
            }

            if (loopCount.HasValue)
            {
                _profileRandom = new Random(_deathSeed + loopCount.Value.GetHashCode());
            }
        }

        private T getValue<T>(Type type, getRandom<T> random)
        {
            if (type == Type.Seed)
            {
                return random.Invoke(_seedRandom);
            }
            if (type == Type.Profile)
            {
                if (_profileRandom == null)
                {
                    var profileName = StandaloneProfileManager.SharedInstance?.currentProfile?.profileName;
                    if (profileName != null) {
                        _profileRandom = new Random(_profileSeed + profileName.GetHashCode());
                    }
                }
                return random.Invoke(_profileRandom);
            }
            if (type == Type.Death)
            {
                if (_deathRandom == null)
                {
                    var loopCount = StandaloneProfileManager.SharedInstance?.currentProfileGameSave?.fullTimeloops;
                    if (loopCount.HasValue)
                    {
                        _deathRandom = new Random(_deathSeed + loopCount.Value.GetHashCode());
                    }
                }
                return random.Invoke(_deathRandom);
            }
            if (type == Type.Minute)
            {
                return random.Invoke(_minuteRandom);
            }
            if (type == Type.Use)
            {
                return random.Invoke(_useRandom);
            }
            return random.Invoke(_seedlessRandom);
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
    }
}

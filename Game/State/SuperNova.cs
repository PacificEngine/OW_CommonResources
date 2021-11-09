using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PacificEngine.OW_CommonResources.Game.State
{
    public static class SuperNova
    {
        public static float maximum { get; set; } = float.PositiveInfinity;
        public static float remaining
        {
            get
            {
                return TimeLoop.GetSecondsRemaining();
            }
            set
            {
                TimeLoop.SetSecondsRemaining(value);
                if (_supernovaTimer >= 0f)
                {
                    _supernovaTimer = value;
                }
            }
        }

        private static float _supernovaTimer = float.NaN;
        public static bool freeze
        {
            get
            {
                return !float.IsNaN(_supernovaTimer);
            }
            set
            {
                if (value)
                {
                    _supernovaTimer = remaining;
                }
                else
                {
                    _supernovaTimer = float.NaN;
                }
            }
        }

        public static void Update()
        {
            if (freeze)
            {
                TimeLoop.SetSecondsRemaining(_supernovaTimer);
            }
            if (remaining > maximum)
            {
                remaining = maximum;
            }
        }
    }
}

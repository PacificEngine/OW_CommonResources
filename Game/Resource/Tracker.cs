using PacificEngine.OW_CommonResources.Game.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Game.Resource
{
    public class TrackedObject
    {
        private HeavenlyBody body;
        private HeavenlyBody previousAstro;

        public delegate void AstroUpdateEvent(HeavenlyBody body, HeavenlyBody previous, HeavenlyBody current);
        public event AstroUpdateEvent onAstroUpdateEvent;

        public TrackedObject(HeavenlyBody body)
        {
            this.body = body;
            this.previousAstro = HeavenlyBody.None;
            Update();
        }

        public void Update()
        {
            var currentAstro = getCurrentAstro();

            if (currentAstro != previousAstro)
            {
                var old = previousAstro;
                previousAstro = currentAstro;
                onAstroUpdateEvent?.Invoke(body, old, currentAstro);
            }
        }

        private HeavenlyBody getCurrentAstro()
        {
            var state = PositionState.fromCurrentState(body);
            if (state == null)
            {
                return HeavenlyBody.None;
            }
            
            var closet = Position.getClosetInfluence(state.position, Position.getAstros(), new HeavenlyBody[0]);
            if (closet != null && closet.Count > 0)
            {
                return closet[0].Item1;
            }

            return HeavenlyBody.None;
        }
    }

    public static class Tracker
    {
        private static float _lastUpdate = 0f;
        private static float _trackingFrequency = 1f;
        private static Dictionary<HeavenlyBody, TrackedObject> bodies = new Dictionary<HeavenlyBody, TrackedObject>();

        public static void Start()
        {
        }

        public static void Awake()
        {
            bodies.Clear();
            _lastUpdate = Time.time;
        }

        public static void Destroy()
        {
        }

        public static void Update()
        {
            if (Time.time - _lastUpdate > _trackingFrequency)
            {
                foreach (var body in bodies)
                {
                    body.Value.Update();
                }
                _lastUpdate = Time.time;
            }
        }

        public static void FixedUpdate()
        {
        }

        public static TrackedObject getTracked(HeavenlyBody body)
        {
            TrackedObject value;
            if (!bodies.TryGetValue(body, out value))
            {
                value = new TrackedObject(body);
                bodies.Add(body, value);
            }

            return value;
        }
    }
}

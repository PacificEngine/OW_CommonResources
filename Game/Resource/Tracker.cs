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
        private Position.HeavenlyBodies body;
        private Position.HeavenlyBodies previousAstro;

        public delegate void AstroUpdateEvent(Position.HeavenlyBodies body, Position.HeavenlyBodies previous, Position.HeavenlyBodies current);
        public event AstroUpdateEvent onAstroUpdateEvent;

        public TrackedObject(Position.HeavenlyBodies body)
        {
            this.body = body;
            this.previousAstro = Position.HeavenlyBodies.None;
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

        private Position.HeavenlyBodies getCurrentAstro()
        {
            var state = PositionState.fromCurrentState(body);
            if (state == null)
            {
                return Position.HeavenlyBodies.None;
            }
            
            var closet = Position.getClosetInfluence(state.position, Position.getAstros(), new Position.HeavenlyBodies[0]);
            if (closet != null && closet.Count > 0)
            {
                return closet[0].Item1;
            }

            return Position.HeavenlyBodies.None;
        }
    }

    public static class Tracker
    {
        private static float _lastUpdate = 0f;
        private static float _trackingFrequency = 1f;
        private static Dictionary<Position.HeavenlyBodies, TrackedObject> bodies = new Dictionary<Position.HeavenlyBodies, TrackedObject>();

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

        public static TrackedObject getTracked(Position.HeavenlyBodies body)
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

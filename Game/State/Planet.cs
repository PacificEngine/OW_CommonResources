using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game.Display;
using PacificEngine.OW_CommonResources.Game.Player;
using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Geometry;
using PacificEngine.OW_CommonResources.Geometry.Orbits;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PacificEngine.OW_CommonResources.Game.State
{
    public static class Planet
    {
        private const string classId = "PacificEngine.OW_CommonResources.Game.State.Planet";

        public class Plantoid
        {
            public Position.Size size { get; }
            public Gravity gravity { get; }
            public RelativeState state { get; }

            public Plantoid(Position.Size size, Gravity gravity, Quaternion orientation, float rotationalSpeed, HeavenlyBody parent, KeplerCoordinates orbit)
            {
                this.size = size;
                this.gravity = gravity;

                var angularVelocity = orientation * (Vector3.up * rotationalSpeed);
                this.state = RelativeState.fromKepler(parent, ScaleState.identity, orbit, new OrientationState(orientation, angularVelocity, Vector3.zero));
            }

            public Plantoid(Position.Size size, Gravity gravity, Quaternion orientation, float rotationalSpeed, HeavenlyBody parent, Vector3 position, Vector3 velocity)
            {
                this.size = size;
                this.gravity = gravity;

                var angularVelocity = orientation * (Vector3.up * rotationalSpeed);
                this.state = RelativeState.fromRelative(parent, new MovementState(ScaleState.identity, new PositionState(position, velocity, Vector3.zero, Vector3.zero), new OrientationState(orientation, angularVelocity, Vector3.zero)));
            }

            public Plantoid(Position.Size size, Gravity gravity, HeavenlyBody parent, OWRigidbody target)
            {
                this.size = size;
                this.gravity = gravity;
                this.state = RelativeState.fromGlobal(parent, target);
            }

            public Plantoid(Position.Size size, Gravity gravity, RelativeState state)
            {
                this.size = size;
                this.gravity = gravity;
                this.state = state;
            }

            public override string ToString()
            { 
                return $"({size}, {gravity}, {state})";
            }

            public override bool Equals(System.Object other)
            {
                if (other != null && other is Plantoid)
                {
                    var obj = other as Plantoid;
                    return size == obj.size
                        && gravity == obj.gravity
                        && state == obj.state;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (size.GetHashCode() * 4)
                    + (gravity.GetHashCode() * 16)
                    + (state.GetHashCode() * 64);
            }
        }

        private static float lastUpdate = 0f;
        private static List<string> debugIds = new List<string>();
        public static bool enabledManagement { get; set; } = true;
        public static int logPlanetPositionFrequency { get; set; } = -1;
        public static bool debugPlanetPosition { get; set; } = false;

        private static Dictionary<HeavenlyBody, Tuple<InitialMotion, Vector3, Vector3, Quaternion, Vector3, GravityVolume>> dict = new Dictionary<HeavenlyBody, Tuple<InitialMotion, Vector3, Vector3, Quaternion, Vector3, GravityVolume>>();
        private static Dictionary<HeavenlyBody, Plantoid> _defaultMapping = standardMapping;
        private static Dictionary<HeavenlyBody, Plantoid> _mapping = standardMapping;
        private static bool update = false;
        private static bool fixUpdate = false;

        public static Dictionary<HeavenlyBody, Plantoid> standardMapping
        {
            get
            {
                var mapping = new Dictionary<HeavenlyBody, Plantoid>();
                mapping.Add(HeavenlyBodies.Sun, new Plantoid(new Position.Size(2000, 50000), Gravity.of(2, 400000000000), new Quaternion(0, 0, 0, 1), 0f, HeavenlyBodies.None, Vector3.zero, Vector3.zero));
                mapping.Add(HeavenlyBodies.SunStation, new Plantoid(new Position.Size(200f, 550), Gravity.of(2, 300000000), new Quaternion(0, 0.707f, 0, -0.707f), 0f, HeavenlyBodies.Sun, KeplerCoordinates.fromTrueAnomaly(0.0002f, 2295.99976f, 0, 180, 0, 90)));
                mapping.Add(HeavenlyBodies.HourglassTwins, new Plantoid(new Position.Size(0f, 692.8f), Gravity.of(1, 800000), new Quaternion(-0.5f, -0.5f, 0.5f, 0.5f), 0f, HeavenlyBodies.Sun, KeplerCoordinates.fromTrueAnomaly(0.0003f, 5000.00098f, 0, 305.2567f, 0, 179.7433f)));
                mapping.Add(HeavenlyBodies.AshTwin, new Plantoid(new Position.Size(200f, 692.8f), Gravity.of(1, 1600000), new Quaternion(0.5f, -0.5f, -0.5f, 0.5f), 0.07f, HeavenlyBodies.HourglassTwins, KeplerCoordinates.fromTrueAnomaly(0, 249.999985f, 180, 33.4767f, 180, 111.5233f)));
                mapping.Add(HeavenlyBodies.EmberTwin, new Plantoid(new Position.Size(200f, 692.8f), Gravity.of(1, 1600000), new Quaternion(0.707f, 0, 0, -0.707f), 0.05f, HeavenlyBodies.HourglassTwins, KeplerCoordinates.fromTrueAnomaly(0, 250.000092f, 180, 292.6138f, 180, 32.3862f)));
                mapping.Add(HeavenlyBodies.TimberHearth, new Plantoid(new Position.Size(250, 1061), Gravity.of(1, 3000000), new Quaternion(0, 0.707f, 0.707f, 0), -0.01f, HeavenlyBodies.Sun, KeplerCoordinates.fromTrueAnomaly(0.0002f, 8593.08594f, 0, 10, 0, 270)));
                mapping.Add(HeavenlyBodies.TimberHearthProbe, new Plantoid(new Position.Size(0.5f, 0.5f), Gravity.of(2, 10), new Quaternion(0, -0.707f, 0, -0.707f), 0f, HeavenlyBodies.TimberHearth, KeplerCoordinates.fromTrueAnomaly(0, 350.100128f, 0, 101.0517f, 0, 89.0675f)));
                mapping.Add(HeavenlyBodies.Attlerock, new Plantoid(new Position.Size(100, 223.6f), Gravity.of(2, 50000000), new Quaternion(0.707f, 0, 0, -0.707f), 0f, HeavenlyBodies.TimberHearth, KeplerCoordinates.fromTrueAnomaly(0, 900.000183f, 0, 36.193f, 0, 333.807f)));
                mapping.Add(HeavenlyBodies.BrittleHollow, new Plantoid(new Position.Size(300, 1162), Gravity.of(1, 3000000), new Quaternion(-0.707f, 0, 0, -0.707f), 0.02f, HeavenlyBodies.Sun, KeplerCoordinates.fromTrueAnomaly(0.0002f, 11690.8896f, 0, 174.1133f, 0, 175.8867f)));
                mapping.Add(HeavenlyBodies.HollowLantern, new Plantoid(new Position.Size(130, 421.2f), Gravity.of(1, 910000), new Quaternion(-1, 0, 0, 0), -0.2f, HeavenlyBodies.BrittleHollow, KeplerCoordinates.fromTrueAnomaly(0, 999.999817f, 0, 161.3635f, 0, 188.6365f)));
                mapping.Add(HeavenlyBodies.GiantsDeep, new Plantoid(new Position.Size(900, 5422), Gravity.of(1, 21780000), new Quaternion(-0.707f, 0, 0, -0.707f), 0f, HeavenlyBodies.Sun, KeplerCoordinates.fromTrueAnomaly(0.0002f, 16457.5898f, 0, 284.0027f, 0, 357.9973f)));
                mapping.Add(HeavenlyBodies.ProbeCannon, new Plantoid(new Position.Size(200, 550), Gravity.of(2, 300000000), Quaternion.identity, 0f, HeavenlyBodies.GiantsDeep, KeplerCoordinates.fromTrueAnomaly(0, 1199.99878f, 180, 214.9022f, 180, 178.0978f)));
                mapping.Add(HeavenlyBodies.DarkBramble, new Plantoid(new Position.Size(650, 1780), Gravity.of(1, 3250000), new Quaternion(0, 0.707f, -0.707f, 0), 0f, HeavenlyBodies.Sun, KeplerCoordinates.fromTrueAnomaly(0.0003f, 20000, 0, 283.2319f, 0, 176.7681f)));
                mapping.Add(HeavenlyBodies.WhiteHole, new Plantoid(new Position.Size(30, 200), Gravity.of(2, 1000000), new Quaternion(0, 0.707f, 0, 0.707f), 0f, HeavenlyBodies.None, new Vector3(-23000, 0, 0), Vector3.zero));
                mapping.Add(HeavenlyBodies.WhiteHoleStation, new Plantoid(new Position.Size(30, 100), Gravity.of(2, 100000), new Quaternion(0, 0.04225808f, 0, -0.9991068f), 0f, HeavenlyBodies.None, new Vector3(-22538.19f, 0, 0), Vector3.zero));
                mapping.Add(HeavenlyBodies.Interloper, new Plantoid(new Position.Size(110, 301.2f), Gravity.of(1, 550000), new Quaternion(0, 1, 0, 0), 0.0034f, HeavenlyBodies.Sun, KeplerCoordinates.fromTrueAnomaly(0.8194f, 13246.3076f, 180, 180, 180, 180)));
                mapping.Add(HeavenlyBodies.Stranger, new Plantoid(new Position.Size(600, 1000), Gravity.of(2, 300000000), new Quaternion(-0.381f, -0.892f, 0.033f, -0.239f), -0.05f, HeavenlyBodies.None, new Vector3(8168.197f, 8400, 2049.528f), Vector3.zero));
                mapping.Add(HeavenlyBodies.DreamWorld, new Plantoid(new Position.Size(1000, 1000), Gravity.of(2, 300000000), new Quaternion(0, 0.087f, 0, -0.996f), 0f, HeavenlyBodies.None, new Vector3(7791.638f, 7000, 1881.588f), Vector3.zero));
                mapping.Add(HeavenlyBodies.QuantumMoon, new Plantoid(new Position.Size(110, 301.2f), Gravity.of(1, 550000), Quaternion.identity, 0f, HeavenlyBodies.None, Vector3.zero, Vector3.zero));
                mapping.Add(HeavenlyBodies.SatiliteBacker, new Plantoid(new Position.Size(5, 100), Gravity.of(2, 100), new Quaternion(0, 0, 0, 1), 0f, HeavenlyBodies.Sun, new Vector3(42000, 5000, -22500), new Vector3(-46.8846f, 28.13076f, 24.70819f)));
                mapping.Add(HeavenlyBodies.SatiliteMapping, new Plantoid(new Position.Size(5, 100), Gravity.of(2, 500), new Quaternion(-0.331f, -0.625f, 0.625f, -0.331f), 0f, HeavenlyBodies.Sun, KeplerCoordinates.fromTrueAnomaly(0, 25999.998f, 90, 254.9042f, 10, 90.0968f)));

                return mapping;
            }
        }

        public static Dictionary<HeavenlyBody, Plantoid> defaultMapping
        {
            get
            {
                Dictionary<HeavenlyBody, Plantoid> value = new Dictionary<HeavenlyBody, Plantoid>();
                foreach(var val in _defaultMapping)
                {
                    value.Add(val.Key, val.Value);
                }
                return value;
            }
            set
            {
                _defaultMapping.Clear();
                foreach (var val in value)
                {
                    _defaultMapping.Add(val.Key, val.Value);
                }
                mapping = _mapping;
            }
        }

        public static Dictionary<HeavenlyBody, Plantoid> mapping
        {
            get
            {
                var mapping = new Dictionary<HeavenlyBody, Plantoid>();
                foreach (HeavenlyBody body in _mapping.Keys)
                {
                    var owBody = Position.getBody(body);
                    if (owBody != null)
                    {
                        var parent = _getParent(body, false);

                        var gravity = _getGravity(body, false);
                        var size = _getSize(body, false);

                        mapping.Add(body, new Plantoid(size, gravity, parent, owBody));
                    }
                    else
                    {
                        mapping.Add(body, new Plantoid(_mapping[body].size, _mapping[body].gravity, _mapping[body].state));
                    }
                }

                return mapping;
            }
            set
            {
                Dictionary<HeavenlyBody, Plantoid> mapping = defaultMapping;
                foreach (var map in value)
                {
                    mapping[map.Key] = map.Value;
                }
                _mapping = mapping;
                update = true;
            }
        }

        public static void Start()
        {
            Helper.helper.HarmonyHelper.AddPrefix<OrbitLine>("Update", typeof(Planet), "onOrbitLineUpdate");
            Helper.helper.HarmonyHelper.AddPrefix<EllipticOrbitLine>("Update", typeof(Planet), "onOrbitLineUpdate");
            Helper.helper.HarmonyHelper.AddPrefix<MapSatelliteOrbitLine>("Update", typeof(Planet), "onOrbitLineUpdate");
            Helper.helper.HarmonyHelper.AddPostfix<OWRigidbody>("Awake", typeof(Planet), "onOWRigidbodyAwake");
            Helper.helper.HarmonyHelper.AddPrefix<InitialMotion>("Start", typeof(Planet), "onInitialMotionStart");
            Helper.helper.HarmonyHelper.AddPrefix<InitialVelocity>("Start", typeof(Planet), "onInitialVelocityStart");
            Helper.helper.HarmonyHelper.AddPrefix<MatchInitialMotion>("Start", typeof(Planet), "onMatchInitialMotionStart");
            Helper.helper.HarmonyHelper.AddPrefix<KinematicRigidbody>("Move", typeof(Planet), "onKinematicRigidbodyMove");

        }

        public static void Awake()
        {
            update = true;
        }

        public static void Destroy()
        {
        }

        public static void SceneLoaded()
        {
            if (logPlanetPositionFrequency > 0)
            {
                Helper.helper.Console.WriteLine($"Scene Loaded Planet State");
                foreach (var body in mapping)
                {
                    var owBody = Position.getBody(body.Key);
                    if (owBody != null)
                    {
                        var parent = _getParent(body.Key, false);

                        var gravity = _getGravity(body.Key, false);
                        var size = _getSize(body.Key, false);
                        var state = RelativeState.fromGlobal(parent, owBody, true);

                        Helper.helper.Console.WriteLine($"{body.Key}: Initial -> {new Plantoid(size, gravity, state)}");
                    }
                    else
                    {
                        Helper.helper.Console.WriteLine($"{body.Key}: Current -> {body.Value}");
                    }
                }
            }
        }

        public static void Update()
        {
            var console = DisplayConsole.getConsole(ConsoleLocation.BottomRight);
            if (Time.time - lastUpdate > 0.2f)
            {
                lastUpdate = Time.time;
                foreach (var id in debugIds)
                {
                    console.setElement(id, "", 0f);
                }
                debugIds.Clear();

                if (debugPlanetPosition)
                {
                    var index = 16.0001f;

                    debugIds.Add(classId + ".Planet.Header");
                    console.setElement(classId + ".Planet.Header", "Planet Coordinates", index);
                    foreach (var map in mapping)
                    {
                        var id = classId + ".Planet." + map.Key;
                        debugIds.Add(id + ".1");
                        debugIds.Add(id + ".2");
                        console.setElement(id + ".1", $" {map.Key.ToString()}: {map.Value.size}, {map.Value.gravity}, {map.Value.state.parent.ToString()}", index + 0.0001f);
                        if (map.Value.state.orbit != null && map.Value.state.orbit.coordinates != null && map.Value.state.orbit.coordinates.isOrbit())
                        {
                            console.setElement(id + ".2", $"{map.Value.state.orbit.coordinates.ToString()}", index + 0.0002f);
                        }
                        else if (map.Value.state.relative != null && map.Value.state.relative.position != null)
                        {
                            console.setElement(id + ".2", $"{map.Value.state.relative.position}", index + 0.0002f);
                        }                        

                        index += 0.0005f;
                    }
                }
            }

            if (GameTimer.FramesSinceAwake > 10)
            {
                updateList();
            }

            if (logPlanetPositionFrequency > 0)
            {
                if (GameTimer.FramesSinceAwake % logPlanetPositionFrequency == 0)
                {
                    Helper.helper.Console.WriteLine($"Frame {GameTimer.FramesSinceAwake} Planet State");
                    foreach (var map in mapping)
                    {
                        Helper.helper.Console.WriteLine($"{map.Key}: {map.Value}");
                    }
                }
            }
        }

        public static void FixedUpdate()
        {
            if (GameTimer.FramesSinceAwake > 2)
            {
                fixUpdateList();
            }
        }

        private static void fixUpdateList()
        {
            if (enabledManagement && fixUpdate)
            {
                fixUpdate = false;
                relocateMovingOrbs();
            }
        }

        private static void updateList()
        {
            if (enabledManagement && update)
            {
                update = false;
                fixUpdate = true;

                var ignorables = new HashSet<OWRigidbody>();
                foreach (var body in _mapping.Keys)
                {
                    ignorables.Add(Position.getBody(body));
                }
                var newState = new Dictionary<HeavenlyBody, AbsoluteState>();
                var movingItems = trackMovingItems(ignorables);
                foreach (var body in _mapping.Keys)
                {
                    var state = updatePlanet(body, newState);
                    if (state != null) {
                        newState.Add(body, state);
                    }
                }
                relocateMovingItems(newState, movingItems);
            }
        }

        private static List<Tuple<OWRigidbody, RelativeState>> trackMovingItems(HashSet<OWRigidbody> ignorables)
        {
            List<Tuple<OWRigidbody, RelativeState>> bodies = new List<Tuple<OWRigidbody, RelativeState>>();
            if (PlayerState.IsInsideTheEye())
            {
                return bodies;
            }
            if (!PlayerState.IsInsideShip() && !PlayerState.IsInsideShuttle())
            {
                bodies.Add(captureState(Position.getBody(HeavenlyBodies.Player)));
            }
            else
            {
                var body = Position.getBody(HeavenlyBodies.Player);
                if (body != null)
                    ignorables.Add(body);
            }
            bodies.Add(captureState(Position.getBody(HeavenlyBodies.Ship)));
            bodies.Add(captureState(Position.getBody(HeavenlyBodies.Probe)));
            bodies.Add(captureState(Position.getBody(HeavenlyBodies.NomaiProbe)));
            bodies.Add(captureState(Position.getBody(HeavenlyBodies.NomaiEmberTwinShuttle)));
            bodies.Add(captureState(Position.getBody(HeavenlyBodies.NomaiBrittleHollowShuttle)));
            foreach (var body in bodies)
            {
                if (body?.Item1 != null)
                    ignorables.Add(body.Item1);
            }
            var modelShip = Position.getBody(HeavenlyBodies.ModelShip);
            if (modelShip != null && modelShip.enabled)
            {
                bodies.Add(captureState(modelShip));
            }
            else
            {
                ignorables.Add(modelShip);
            }

            foreach (var child in GameObject.FindObjectsOfType<OWRigidbody>())
            {
                var name = child?.gameObject?.name;
                var parentName = child?.GetOrigParentBody()?.gameObject?.name;
                if (name == null)
                {
                    continue;
                }
                if (parentName != null)
                {
                    if (parentName.StartsWith("SunStation_Body")
                        && (name.StartsWith("SS_Debris_Body")))
                    {
                        if (child != null && child.enabled)
                        {
                            var surface = RelativeState.getSurfaceMovement(HeavenlyBodies.SunStation, child);
                            bodies.Add(Tuple.Create(child, RelativeState.fromSurface(HeavenlyBodies.SunStation, surface)));
                        }
                        continue;
                    }
                    if (parentName.StartsWith("TowerTwin_Body")
                        && (name.StartsWith("TimeLoopRing_Body")))
                    {
                        var relative = RelativeState.getRelativeMovement(HeavenlyBodies.AshTwin, child);
                        bodies.Add(Tuple.Create(child, RelativeState.fromSurface(HeavenlyBodies.AshTwin, relative)));
                        continue;
                    }
                    if (parentName.StartsWith("OrbitalProbeCannon_Body")
                        && (name.StartsWith("Debris_Body")
                            || name.StartsWith("FakeCannonMuzzle_Body")
                            || name.StartsWith("FakeCannonBarrel_Body")))
                    {
                        if (child != null && child.enabled)
                        {
                            var surface = RelativeState.getSurfaceMovement(HeavenlyBodies.ProbeCannon, child);
                            bodies.Add(Tuple.Create(child, RelativeState.fromSurface(HeavenlyBodies.ProbeCannon, surface)));
                        }
                        continue;
                    }
                    if (parentName.StartsWith("GiantsDeep_Body")
                        && (name.StartsWith("GabbroShip_Body")
                            || name.StartsWith("StatueIsland_Body")
                            || name.StartsWith("ConstructionYardIsland_Body")
                            || name.StartsWith("GabbroIsland_Body")
                            || name.StartsWith("QuantumIsland_Body")
                            || name.StartsWith("BrambleIsland_Body")))
                    {
                        bodies.Add(captureState(child, HeavenlyBodies.GiantsDeep));
                        continue;
                    }
                    if (parentName.StartsWith("WhiteholeStation_Body")
                        && (name.StartsWith("WhiteholeStationSuperstructure_Body")))
                    {
                        bodies.Add(captureState(child, HeavenlyBodies.WhiteHoleStation));
                        continue;
                    }
                }
                else
                {
                    if (name.StartsWith("CannonBarrel_Body")
                         && child != null && child.enabled)
                    {
                        // CannonBarrel_Body has no parent
                        if (child != null && child.enabled)
                        {
                            var surface = RelativeState.getSurfaceMovement(HeavenlyBodies.ProbeCannon, child);
                            bodies.Add(Tuple.Create(child, RelativeState.fromSurface(HeavenlyBodies.ProbeCannon, surface)));
                        }
                        continue;
                    }
                }

                if (child.GetComponentInChildren<NomaiInterfaceOrb>(true) != null)
                {
                    // Handle Orbs Afterwards
                    continue;
                }

                if (!ignorables.Contains(child) && child != null && child.enabled)
                {
                    bodies.Add(captureState(child));
                }
            }

            return bodies;
        }

        private static void relocateMovingOrbs()
        {
            foreach (var child in Resources.FindObjectsOfTypeAll<NomaiInterfaceOrb>())
            {
                var body = child.GetValue<OWRigidbody>("_orbBody");
                if (child.GetValue<bool>("_isBeingDragged"))
                {
                    body.SetPosition(body.GetOrigParent().TransformPoint(child.GetValue<Vector3>("_localTargetPos")));
                }
                else
                {
                    var slot = child.GetCurrentSlot();
                    if (slot != null)
                    {
                        body.SetPosition(slot.transform.position);
                    }
                    else
                    {
                        var slots = child.GetValue<NomaiInterfaceSlot[]>("_slots");
                        if (slots != null && slots.Length > 0)
                        {
                            body.SetPosition(slots[0].transform.position);
                        }
                    }
                }
            }
        }

        private static void relocateMovingItems(Dictionary<HeavenlyBody, AbsoluteState> newStates, List<Tuple<OWRigidbody, RelativeState>> movingItems)
        {
            foreach (var movingItem in movingItems)
            {
                if (movingItem == null || movingItem.Item1 == null || movingItem.Item2 == null)
                {
                    continue;
                }

                var parentState = !newStates.ContainsKey(movingItem.Item2.parent)
                    ? AbsoluteState.fromCurrentState(movingItem.Item2.parent)
                    : newStates[movingItem.Item2.parent];
                var parentGravity = _getGravity(movingItem.Item2.parent, true);
                if (parentState == null || parentGravity == null)
                {
                    continue;
                }
                movingItem.Item2.apply(movingItem.Item1, parentState, parentGravity);
            }
        }

        private static Tuple<OWRigidbody, RelativeState> captureState(OWRigidbody item, HeavenlyBody parent)
        {
            if (item == null)
            {
                return null;
            }

            return Tuple.Create(item, RelativeState.fromGlobal(parent, item));
        }

        private static Tuple<OWRigidbody, RelativeState> captureState(OWRigidbody item)
        {
            if (item == null)
            {
                return null;
            }

            return Tuple.Create(item, RelativeState.fromClosetInfluence(item, HeavenlyBodies.Sun,
                HeavenlyBodies.Player,
                HeavenlyBodies.Probe,
                HeavenlyBodies.Ship,
                HeavenlyBodies.ModelShip,
                HeavenlyBodies.NomaiProbe,
                HeavenlyBodies.NomaiBrittleHollowShuttle,
                HeavenlyBodies.NomaiEmberTwinShuttle,
                HeavenlyBodies.TimberHearthProbe,
                HeavenlyBodies.EyeOfTheUniverse,
                HeavenlyBodies.EyeOfTheUniverse_Vessel));
        }

        public static List<HeavenlyBody> getChildren(HeavenlyBody parent)
        {
            return _getChildren(parent, false);
        }

        private static List<HeavenlyBody> _getChildren(HeavenlyBody parent, bool stateOnly)
        {
            var children = new List<HeavenlyBody>();
            foreach (var child in Position.getAstros())
            {
                if (_getParent(child, stateOnly) == parent)
                {
                    children.Add(child);
                }
            }

            return children;
        }

        public static HeavenlyBody getParent(HeavenlyBody child)
        {
            return _getParent(child, false);
        }

        private static HeavenlyBody _getParent(HeavenlyBody child, bool stateOnly)
        {
            var parent = HeavenlyBody.None;
            if (child == null)
            {
                return HeavenlyBody.None;
            }

            if (!stateOnly)
            {
                parent = _getParentCurrent(child);
                if (parent != HeavenlyBody.None)
                {
                    return parent;
                }
            }

            parent = _getParentState(child);
            if (parent != HeavenlyBody.None)
            {
                return parent;
            }

            return HeavenlyBody.None;
        }

        private static HeavenlyBody _getParentCurrent(HeavenlyBody child)
        {
            var astroChild = Position.getAstro(child);
            if (astroChild == null)
            {
                return HeavenlyBody.None;
            }

            var astroParent = astroChild.GetPrimaryBody();
            var parent = Position.find(astroParent);
            if (parent == null)
            {
                return HeavenlyBody.None;
            }

            return parent;
        }

        private static HeavenlyBody _getParentState(HeavenlyBody child)
        {
            if (child == null)
            {
                return HeavenlyBody.None;
            }
            else if (_mapping.ContainsKey(child))
            {
                var childMap = _mapping[child];
                var parent = childMap?.state?.parent;
                if (parent == null)
                {
                    return HeavenlyBody.None;
                }
                return parent;
            }

            return HeavenlyBody.None;
        }

        public static Gravity getParentGravity(HeavenlyBody child)
        {
            return _getParentGravity(child, false);
        }

        private static Gravity _getParentGravity(HeavenlyBody child, bool stateOnly)
        {
            var parent = _getParent(child, stateOnly);
            if (parent == null)
            {
                return null;
            }
            else if (parent.pseudoHeavenlyBody)
            {
                float childDistance;
                var parentState = stateOnly ? null : AbsoluteState.fromCurrentState(parent);
                if (parentState != null)
                {
                    var childState = AbsoluteState.fromCurrentState(child);
                    childDistance = (childState.position - parentState.position).magnitude;
                }
                else if (_mapping.ContainsKey(child))
                {
                    childDistance = _mapping[child]?.state?.orbit?.coordinates?.perigee ?? 0f;
                }
                else
                {
                    childDistance = 0f;
                }
                var siblings = _getChildren(parent, stateOnly);
                float mass = 0;
                foreach (var sibling in siblings)
                {
                    if (sibling == child)
                    {
                        continue;
                    }
                    var siblingGravity = _getGravity(sibling, stateOnly);

                    float siblingDistance;
                    if (parentState != null)
                    {
                        var siblingState = AbsoluteState.fromCurrentState(sibling);
                        siblingDistance = (siblingState.position - parentState.position).magnitude;
                    }
                    else if (_mapping.ContainsKey(sibling))
                    {
                        siblingDistance = _mapping[sibling]?.state?.orbit?.coordinates?.perigee ?? 0f;
                    }
                    else
                    {
                        siblingDistance = 0f;
                    }

                    mass += (siblingGravity.mu * childDistance * childDistance) / (((float)Math.Pow(childDistance + siblingDistance, siblingGravity.exponent)) * Gravity.GRAVITATIONAL_CONSTANT);
                }
                return Gravity.of(2, mass);
            }
            
            return _getGravity(parent, stateOnly);
        }

        public static Gravity getGravity(HeavenlyBody body)
        {
            return _getGravity(body, false);
        }

        private static Gravity _getGravity(HeavenlyBody parent, bool stateOnly)
        {
            var gravity = Gravity.of(2f, 0f);
            if (parent == null || parent.pseudoHeavenlyBody)
            {
                return Gravity.of(2f, 0f);
            }

            if (!stateOnly)
            {
                gravity = _getGravityCurrent(parent);
                if (gravity != null)
                {
                    return gravity;
                }
            }

            gravity = _getGravityState(parent);
            if (gravity != null)
            {
                return gravity;
            }

            if (!stateOnly)
            {
                var parentBody = Position.getBody(parent);
                if (parentBody != null)
                {
                    return Gravity.of(2f, parentBody.GetMass());
                }
            }

            return Gravity.of(2f, 0f);
        }

        private static Gravity _getGravityCurrent(HeavenlyBody parent)
        {
            if (parent == null || parent.pseudoHeavenlyBody)
            {
                return null;
            }

            var parentBody = Position.getBody(parent);
            if (parentBody == null)
            {
                return null;
            }

            if (parentBody?.GetAttachedGravityVolume() == null)
            {
                return null;
            }

            var exponent = parentBody?.GetAttachedGravityVolume()?.GetValue<float>("_falloffExponent") ?? 2f;
            var mass = parentBody?.GetAttachedGravityVolume()?.GetValue<float>("_gravitationalMass") ?? ((parentBody?.GetMass() ?? 0f) * 1000f);
            return Gravity.of(exponent, mass);
        }

        private static Gravity _getGravityState(HeavenlyBody parent)
        {
            if (parent == null || parent.pseudoHeavenlyBody)
            {
                return null;
            }
            else if (_mapping.ContainsKey(parent))
            {
                var parentMap = _mapping[parent];
                return Gravity.of(parentMap.gravity.exponent, parentMap.gravity.mass);
            }

            return null;
        }

        public static Position.Size getSize(HeavenlyBody child)
        {
            return _getSize(child, false);
        }

        private static Position.Size _getSize(HeavenlyBody child, bool stateOnly)
        {
            if (child == null)
            {
                return new Position.Size(0, 0);
            }

            var size = _getSizeState(child);
            if (size != null)
            {
                return size;
            }

            if (!stateOnly)
            {
                size = _getSizeCurrent(child);
                if (size != null)
                {
                    return size;
                }
            }

            return new Position.Size(0, 0);
        }


        private static Position.Size _getSizeCurrent(HeavenlyBody child)
        {
            if (child == null || child.pseudoHeavenlyBody)
            {
                return null;
            }

            float size;
            float influence;
            if (child == HeavenlyBodies.InnerDarkBramble_Hub
                || child == HeavenlyBodies.InnerDarkBramble_Nest
                || child == HeavenlyBodies.InnerDarkBramble_Feldspar
                || child == HeavenlyBodies.InnerDarkBramble_Gutter
                || child == HeavenlyBodies.InnerDarkBramble_Vessel
                || child == HeavenlyBodies.InnerDarkBramble_Maze
                || child == HeavenlyBodies.InnerDarkBramble_SmallNest
                || child == HeavenlyBodies.InnerDarkBramble_Secret)
            {
                var outerPortal = BramblePortals.getOuterPortal(child, 0) ?? BramblePortals.getOuterPortal(child, -1);
                if (outerPortal != null)
                {
                    size = Mathf.Max(outerPortal.GetWarpRadius(), outerPortal.GetExitRadius());
                    influence = size;
                }
                else
                {
                    var gravity = _getGravity(child, false);
                    size = 0f;
                    influence = (float)Math.Sqrt(gravity.mu);
                }
            }
            else if (child == HeavenlyBodies.Player)
            {
                size = 1f;
                influence = 1f;
            }
            else if (child == HeavenlyBodies.Ship
                || child == HeavenlyBodies.NomaiEmberTwinShuttle
                || child == HeavenlyBodies.NomaiBrittleHollowShuttle)
            {
                size = 1f;
                influence = 1f;
            }
            else if (child == HeavenlyBodies.Player)
            {
                size = 1f;
                influence = 1f;
            }
            else if (child == HeavenlyBodies.Probe
                || child == HeavenlyBodies.ModelShip
                || child == HeavenlyBodies.TimberHearthProbe)
            {
                size = 0.5f;
                influence = 0.5f;
            }
            else if (child == HeavenlyBodies.SunStation
                || child == HeavenlyBodies.ProbeCannon)
            {
                size = 200f;
                influence = 550f;
            }
            else if (child == HeavenlyBodies.NomaiProbe)
            {
                size = 35f;
                influence = 100f;
            }
            else if (child == HeavenlyBodies.WhiteHole)
            {
                size = 30f;
                influence = 200f;
            }
            else if (child == HeavenlyBodies.WhiteHoleStation)
            {
                size = 30f;
                influence = 100f;
            }
            else if (child == HeavenlyBodies.Stranger)
            {
                size = 600f;
                influence = 1000f;
            }
            else if (child == HeavenlyBodies.DreamWorld)
            {
                size = 1000f;
                influence = 1000f;
            }
            else if (child == HeavenlyBodies.SatiliteBacker
                || child == HeavenlyBodies.SatiliteMapping)
            {
                size = 5f;
                influence = 100f;
            }
            else if (child == HeavenlyBodies.EyeOfTheUniverse_Vessel)
            {
                size = 250f;
                influence = 250f;
            }
            else if (child == HeavenlyBodies.Sun)
            {
                size = 2000f;
                influence = 45000f;
            }
            else
            {
                var gravity = _getGravity(child, false);
                var childBody = Position.getBody(child);
                size = Mathf.Max(childBody?.GetAttachedGravityVolume()?.GetValue<float>("_upperSurfaceRadius") ?? 0f,
                                childBody?.GetAttachedGravityVolume()?.GetValue<float>("_lowerSurfaceRadius") ?? 0f,
                                childBody?.GetAttachedGravityVolume()?.GetValue<float>("_cutoffRadius") ?? 0f);

                if (size == 0)
                {
                    size = childBody?.GetAttachedGravityVolume()?.GetValue<float>("_alignmentRadius") ?? 0f;
                }

                if (gravity.exponent < 1.5f && size > 0.01f)
                {
                    influence = (float)Math.Sqrt(gravity.mu * size * 1.5f);
                }
                else
                {
                    influence = (float)Math.Sqrt(gravity.mu);
                }

                if (influence < size)
                {
                    influence = size;
                }
            }

            return new Position.Size(size, influence);
        }


        private static Position.Size _getSizeState(HeavenlyBody child)
        {
            if (child == null || child.pseudoHeavenlyBody)
            {
                return null;
            }
            else if (_mapping.ContainsKey(child))
            {
                var childMap = _mapping[child];
                return new Position.Size(childMap?.size?.size ?? 0f, childMap?.size?.influence ?? 0f);
            }

            return null;
        }

        private static AbsoluteState updatePlanet(HeavenlyBody body, Dictionary<HeavenlyBody, AbsoluteState> newStates)
        {
            var owBody = Position.getBody(body);
            if (owBody == null || !_mapping.ContainsKey(body))
            {
                return null;
            }
            var planet = _mapping[body];

            updatePlanetGravity(planet, owBody);
            if (body == HeavenlyBodies.QuantumMoon)
            {
                return null;
            }
            updatePlanetParent(planet.state.parent, owBody);

            var gravity = _getParentGravity(body, true);
            AbsoluteState parentState = null;
            if (newStates.ContainsKey(planet.state.parent))
            {
                parentState = newStates[planet.state.parent];
            };

            return updatePlanetPosition(parentState, gravity, planet.state, owBody, false);
        }

        private static void updatePlanetGravity(Plantoid planet, OWRigidbody owBody)
        {
            var gravityVolumne = owBody?.GetAttachedGravityVolume();
            if (gravityVolumne != null)
            {
                var _upperSurfaceRadius = gravityVolumne.GetValue<float>("_upperSurfaceRadius");
                var _surfaceAcceleration = (GravityVolume.GRAVITATIONAL_CONSTANT * planet.gravity.mass) / Mathf.Pow(_upperSurfaceRadius, planet.gravity.exponent);

                gravityVolumne.SetValue("_falloffExponent", planet.gravity.exponent);
                gravityVolumne.SetValue("_gravitationalMass", planet.gravity.mass);
                gravityVolumne.SetValue("_surfaceAcceleration", _surfaceAcceleration);
            }
            owBody.SetMass(planet.gravity.mass);
        }

        private static void updatePlanetParent(HeavenlyBody parent, OWRigidbody owBody)
        {
            // TODO: Allow different gravity parent
        }

        private static AbsoluteState updatePlanetPosition(AbsoluteState parentState, Gravity gravity, RelativeState relativeState, OWRigidbody owBody, bool ignoreOrientation)
        {
            return relativeState.apply(owBody, parentState, gravity);
        }

        private static bool onOrbitLineUpdate(ref OrbitLine __instance)
        {
            var _astroObject = __instance?.GetValue<AstroObject>("_astroObject");
            var _lineRenderer = __instance?.GetValue<LineRenderer>("_lineRenderer");
            if (_astroObject == null || _lineRenderer == null)
            {
                return false;
            }
            _lineRenderer.startColor = Color.clear;
            _lineRenderer.endColor = Color.clear;

            AstroObject parentAstro = _astroObject?.GetPrimaryBody();
            if (__instance == null
                || __instance.transform == null
                || parentAstro == null
                || parentAstro.transform == null)
            {
                return false;
            }

            var body = Position.find(_astroObject);
            if (body == null)
            {
                return false;
            }

            Plantoid planet;
            if (!_mapping.TryGetValue(body, out planet))
            {
                return false;
            }

            var parent = planet?.state?.parent;
            var targetState = PositionState.fromCurrentState(body);
            var parentState = AbsoluteState.fromCurrentState(parent);
            var parentGravity = _getParentGravity(body, true);
            if (targetState == null || parentState == null || parentGravity == null)
            {
                return false;
            }

            var kepler = Position.getKepler(parentState, parentGravity, targetState.position, targetState.velocity);
            if (kepler == null && !kepler.isOrbit())
            {
                return false;
            }

            var _numVerts = __instance.GetValue<int>("_numVerts");
            var _verts = new Vector3[_numVerts];            

            var angle = KeplerCoordinates.shiftTimeSincePeriapsis(parentGravity, kepler, Time.timeSinceLevelLoad).esccentricAnomaly;
            var increment = Circle.getPercentageAngle(1f / (float)(_numVerts - 1));
            for (int index = 0; index < _numVerts; ++index)
            {
                var vert = kepler.ellipse.getCoordinatesFromCenterAngle((angle + 180) - (index * increment));
                _verts[index] = new Vector3(vert.y, 0, vert.x);
            }
            _lineRenderer.SetPositions(_verts);

            var periapsis = OrbitHelper.getPeriapsis(parentGravity, kepler);
            var semiMinorDecending = OrbitHelper.getSemiMinorDecending(parentGravity, kepler);
            var _semiMajorAxis = periapsis.Item1.normalized * kepler.semiMajorRadius;
            var _semiMinorAxis = semiMinorDecending.Item1.normalized * kepler.semiMinorRadius;
            var _upAxisDir = Vector3.Cross(periapsis.Item1, semiMinorDecending.Item1);

            Vector3 foci = parentAstro.transform.position - periapsis.Item1.normalized * kepler.foci;

            __instance.transform.position = foci;
            __instance.transform.rotation = Quaternion.LookRotation(foci - parentAstro.transform.position, _upAxisDir);

            var _lineWidth = __instance.GetValue<float>("_lineWidth");
            var _maxLineWidth = __instance.GetValue<float>("_maxLineWidth");
            var _fade = __instance.GetValue<bool>("_fade");
            var _fadeStartDist = __instance.GetValue<float>("_fadeStartDist");
            var _fadeEndDist = __instance.GetValue<float>("_fadeEndDist");
            var _color = __instance.GetValue<Color>("_color");

            float ellipticalOrbitLine = DistanceToEllipticalOrbitLine(foci, _semiMajorAxis, _semiMinorAxis, _upAxisDir, Locator.GetActiveCamera().transform.position);
            float num1 = Mathf.Min(ellipticalOrbitLine * (_lineWidth / 1000f), _maxLineWidth);
            float num2 = _fade ? 1f - Mathf.Clamp01((ellipticalOrbitLine - _fadeStartDist) / (_fadeEndDist - _fadeStartDist)) : 1f;
            _lineRenderer.widthMultiplier = num1;
            if (_color != null)
            {
                _lineRenderer.startColor = new Color(_color.r, _color.g, _color.b, num2 * num2);
            }

            return false;
        }

        private static float CalcProjectedAngleToCenter(Vector3 foci, Vector3 semiMajorAxis, Vector3 semiMinorAxis, Vector3 point)
        {
            Vector3 lhs = point - foci;
            Vector3 vector3 = new Vector3(Vector3.Dot(lhs, semiMajorAxis.normalized), 0.0f, Vector3.Dot(lhs, semiMinorAxis.normalized));
            vector3.x *= semiMinorAxis.magnitude / semiMajorAxis.magnitude;
            return (float)Math.Atan2(vector3.z, vector3.x);
        }

        private static float DistanceToEllipticalOrbitLine(Vector3 foci, Vector3 semiMajorAxis, Vector3 semiMinorAxis, Vector3 upAxis, Vector3 point)
        {
            float center = CalcProjectedAngleToCenter(foci, semiMajorAxis, semiMinorAxis, point);
            Vector3 b = foci + semiMajorAxis * Mathf.Cos(center) + semiMinorAxis * Mathf.Sin(center);
            return Vector3.Distance(point, b);
        }

        private static void onOWRigidbodyAwake(ref OWRigidbody __instance)
        {
            var body = Position.find(__instance);
            if (_mapping.ContainsKey(body))
            {
                update = true;
            }
        }

        private static bool onInitialMotionStart(ref InitialMotion __instance)
        {
            var owBody = __instance.GetValue<OWRigidbody>("_satelliteBody");
            var body = Position.find(owBody);
            if (_mapping.ContainsKey(body))
            {
                update = true;
            }

            return true;
        }

        private static bool onInitialVelocityStart(ref InitialVelocity __instance)
        {
            var owBody = __instance.GetValue<OWRigidbody>("_owRigidbody");
            var body = Position.find(owBody);
            if (_mapping.ContainsKey(body))
            {
                update = true;
            }

            return true;
        }

        private static bool onMatchInitialMotionStart(ref MatchInitialMotion __instance)
        {
            var owBody = __instance.GetValue<OWRigidbody>("_owRigidbody");
            var body = Position.find(owBody);
            if (_mapping.ContainsKey(body))
            {
                update = true;
            }

            return true;
        }

        private static bool onKinematicRigidbodyMove(ref KinematicRigidbody __instance, ref Vector3 position, ref Quaternion rotation)
        {
            var owBody = __instance.GetValue<Rigidbody>("_rigidbody");

            owBody.MovePosition(position);
            owBody.MoveRotation(rotation.normalized);

            return false;
        }
    }
}

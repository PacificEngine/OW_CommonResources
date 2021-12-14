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

            public Plantoid(Position.Size size, Gravity gravity, Quaternion orientation, float rotationalSpeed, Position.HeavenlyBodies parent, KeplerCoordinates orbit)
            {
                this.size = size;
                this.gravity = gravity;

                var angularVelocity = orientation * (Vector3.up * rotationalSpeed);
                this.state = RelativeState.fromKepler(parent, ScaleState.identity, orbit, new OrientationState(orientation, angularVelocity, Vector3.zero));
            }

            public Plantoid(Position.Size size, Gravity gravity, Quaternion orientation, float rotationalSpeed, Position.HeavenlyBodies parent, Vector3 position, Vector3 velocity)
            {
                this.size = size;
                this.gravity = gravity;

                var angularVelocity = orientation * (Vector3.up * rotationalSpeed);
                this.state = RelativeState.fromRelative(parent, new MovementState(ScaleState.identity, new PositionState(position, velocity, Vector3.zero, Vector3.zero), new OrientationState(orientation, angularVelocity, Vector3.zero)));
            }

            public Plantoid(Position.Size size, Gravity gravity, Position.HeavenlyBodies parent, OWRigidbody target)
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
        public static bool debugPlanetPosition { get; set; } = false;

        private static Dictionary<Position.HeavenlyBodies, Tuple<InitialMotion, Vector3, Vector3, Quaternion, Vector3, GravityVolume>> dict = new Dictionary<Position.HeavenlyBodies, Tuple<InitialMotion, Vector3, Vector3, Quaternion, Vector3, GravityVolume>>();
        private static Dictionary<Position.HeavenlyBodies, Plantoid> _mapping = defaultMapping;
        private static List<Tuple<OWRigidbody, RelativeState>> _movingItems = new List<Tuple<OWRigidbody, RelativeState>>();
        private static Dictionary<Position.HeavenlyBodies, AbsoluteState> _newState = new Dictionary<Position.HeavenlyBodies, AbsoluteState>();
        private static bool update = false;


        public static Dictionary<Position.HeavenlyBodies, Plantoid> mapping
        {
            get
            {
                var original = defaultMapping;
                var mapping = new Dictionary<Position.HeavenlyBodies, Plantoid>();
                foreach (Position.HeavenlyBodies body in _mapping.Keys)
                {
                    var owBody = Position.getBody(body);
                    if (owBody != null)
                    {
                        var parent = _mapping[body].state.parent;

                        var gravity = Position.getGravity(body);
                        var size = Position.getSize(body);

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
                Dictionary<Position.HeavenlyBodies, Plantoid> mapping = defaultMapping;
                foreach(var map in value)
                {
                    mapping[map.Key] = map.Value;
                }
                _mapping = mapping;
                update = true;
                updateList();
            }
        }

        public static Dictionary<Position.HeavenlyBodies, Plantoid> defaultMapping
        {
            get
            {
                var sunGravity = Gravity.of(2, 400000000000);
                var hourGlassTwinGravity = Gravity.of(1, 800000);
                var timberHearthGravity = Gravity.of(1, 3000000);
                var brittleHollowGravity = Gravity.of(1, 3000000);
                var giantsDeepGravity = Gravity.of(1, 21780000);

                var mapping = new Dictionary<Position.HeavenlyBodies, Plantoid>();
                mapping.Add(Position.HeavenlyBodies.Sun, new Plantoid(new Position.Size(2000, 20000), Gravity.of(2, 400000000000), new Quaternion(0, 0, 0, 1), 0f, Position.HeavenlyBodies.None, Vector3.zero, Vector3.zero));
                mapping.Add(Position.HeavenlyBodies.SunStation, new Plantoid(new Position.Size(200f, 550), Gravity.of(2, 300000000), new Quaternion(0.502f, 0.502f, -0.498f, -0.498f), 0.1817f, Position.HeavenlyBodies.Sun, KeplerCoordinates.fromTimeSincePeriapsis(sunGravity, 0.0061f, 2296.04395f, 90, 180.3505f, 0, 8.54975f)));
                mapping.Add(Position.HeavenlyBodies.HourglassTwins, new Plantoid(new Position.Size(0f, 692.8f), Gravity.of(1, 800000), new Quaternion(0, -0.887f, 0, 0.462f), 0f, Position.HeavenlyBodies.Sun, KeplerCoordinates.fromTimeSincePeriapsis(sunGravity, 0.0019f, 5000.00879f, 90, 35.1033f, 0, 27.67817f)));
                mapping.Add(Position.HeavenlyBodies.AshTwin, new Plantoid(new Position.Size(200f, 692.8f), Gravity.of(1, 1600000), new Quaternion(0, 0.954f, 0, 0.298f), 0.07f, Position.HeavenlyBodies.HourglassTwins, KeplerCoordinates.fromTimeSincePeriapsis(hourGlassTwinGravity, 0, 249.998428f, 90, 55.4224f, 180, 13.82798f)));
                mapping.Add(Position.HeavenlyBodies.EmberTwin, new Plantoid(new Position.Size(200f, 692.8f), Gravity.of(1, 1600000), new Quaternion(0, -0.886f, 0, 0.463f), 0.05f, Position.HeavenlyBodies.HourglassTwins, KeplerCoordinates.fromTimeSincePeriapsis(hourGlassTwinGravity, 0, 249.998764f, 90, 235.4222f, 180, 13.82804f)));
                mapping.Add(Position.HeavenlyBodies.TimberHearth, new Plantoid(new Position.Size(250, 1061), Gravity.of(1, 3000000), new Quaternion(0, 0.996f, 0, 0.087f), 0.01f, Position.HeavenlyBodies.Sun, KeplerCoordinates.fromTimeSincePeriapsis(sunGravity, 0.0009f, 8593.08984f, 90, 190.0414f, 0, 62.4697f)));
                mapping.Add(Position.HeavenlyBodies.TimberHearthProbe, new Plantoid(new Position.Size(0.5f, 0.5f), Gravity.of(2, 10), new Quaternion(0.704f, -0.704f, 0.064f, -0.064f), 0f, Position.HeavenlyBodies.TimberHearth, KeplerCoordinates.fromTimeSincePeriapsis(timberHearthGravity, 0, 350.097656f, 90, 100.5856f, 0, 9.98418f)));
                mapping.Add(Position.HeavenlyBodies.Attlerock, new Plantoid(new Position.Size(100, 223.6f), Gravity.of(2, 50000000), new Quaternion(0, -0.642f, 0, -0.767f), 0.0609f, Position.HeavenlyBodies.TimberHearth, KeplerCoordinates.fromTimeSincePeriapsis(timberHearthGravity, 0, 899.999085f, 90, 280.2294f, 0, 25.75421f)));
                mapping.Add(Position.HeavenlyBodies.BrittleHollow, new Plantoid(new Position.Size(300, 1162), Gravity.of(1, 3000000), new Quaternion(0, 0.642f, 0, -0.766f), 0.02f, Position.HeavenlyBodies.Sun, KeplerCoordinates.fromTimeSincePeriapsis(sunGravity, 0.0006f, 11690.8877f, 90, 259.9969f, 0, 99.21703f)));
                mapping.Add(Position.HeavenlyBodies.HollowLantern, new Plantoid(new Position.Size(130, 421.2f), Gravity.of(1, 910000), new Quaternion(-0.542f, 0.449f, -0.455f, -0.546f), 0.2f, Position.HeavenlyBodies.BrittleHollow, KeplerCoordinates.fromTimeSincePeriapsis(brittleHollowGravity, 0, 1000.00073f, 90, 260.2069f, 0, 28.62203f)));
                mapping.Add(Position.HeavenlyBodies.GiantsDeep, new Plantoid(new Position.Size(900, 5422), Gravity.of(1, 21780000), new Quaternion(0, 0.105f, 0, -0.995f), 0f, Position.HeavenlyBodies.Sun, KeplerCoordinates.fromTimeSincePeriapsis(sunGravity, 0.0003f, 16457.5918f, 90, 192.0577f, 0, 165.65816f)));
                mapping.Add(Position.HeavenlyBodies.ProbeCannon, new Plantoid(new Position.Size(200, 550), Gravity.of(2, 300000000), new Quaternion(0.551f, 0.408f, -0.527f, 0.503f), 0f, Position.HeavenlyBodies.GiantsDeep, KeplerCoordinates.fromTimeSincePeriapsis(giantsDeepGravity, 0, 1199.99414f, 90, 303.4643f, 180, 12.71561f)));
                mapping.Add(Position.HeavenlyBodies.DarkBramble, new Plantoid(new Position.Size(650, 1780), Gravity.of(1, 3250000), new Quaternion(0, 0.996f, 0, 0.087f), 0f, Position.HeavenlyBodies.Sun, KeplerCoordinates.fromTimeSincePeriapsis(sunGravity, 0.0003f, 20000.0039f, 90, 10.0033f, 0, 222.06259f)));
                mapping.Add(Position.HeavenlyBodies.WhiteHole, new Plantoid(new Position.Size(30, 200), Gravity.of(2, 1000000), new Quaternion(0, 0.7071068f, 0, 0.7071068f), 0f, Position.HeavenlyBodies.Sun, new Vector3(-23000, 0, 0), Vector3.zero));
                mapping.Add(Position.HeavenlyBodies.WhiteHoleStation, new Plantoid(new Position.Size(30, 100), Gravity.of(2, 100000), new Quaternion(0, 0.04225808f, 0, -0.9991068f), 0f, Position.HeavenlyBodies.Sun, new Vector3(-22538.19f, 0, 0), Vector3.zero));
                mapping.Add(Position.HeavenlyBodies.Interloper, new Plantoid(new Position.Size(110, 301.2f), Gravity.of(1, 550000), new Quaternion(0, 1, 0, 0), 0.0034f, Position.HeavenlyBodies.Sun, KeplerCoordinates.fromTimeSincePeriapsis(sunGravity, 0.8194f, 13246.3066f, 90, 180.0053f, 180, 239.44463f)));
                mapping.Add(Position.HeavenlyBodies.Stranger, new Plantoid(new Position.Size(600, 1000), Gravity.of(2, 300000000), new Quaternion(-0.381f, -0.892f, 0.034f, -0.241f), 0.05f, Position.HeavenlyBodies.Sun, new Vector3(8168.197f, 8399.999f, 2049.527f), Vector3.zero));
                mapping.Add(Position.HeavenlyBodies.DreamWorld, new Plantoid(new Position.Size(1000, 1000), Gravity.of(2, 300000000), new Quaternion(0, 0.087f, 0, -0.996f), 0.05f, Position.HeavenlyBodies.Sun, new Vector3(7791.638f, 7000, 1881.588f), Vector3.zero));
                mapping.Add(Position.HeavenlyBodies.QuantumMoon, new Plantoid(new Position.Size(110, 301.2f), Gravity.of(1, 550000), new Quaternion(0.296f, 0.062f, 0.641f, -0.706f), 0f, Position.HeavenlyBodies.None, Vector3.zero, Vector3.zero));
                mapping.Add(Position.HeavenlyBodies.BackerSatilite, new Plantoid(new Position.Size(5, 100), Gravity.of(2, 100), new Quaternion(0, 0, 0, 1), 0f, Position.HeavenlyBodies.Sun, KeplerCoordinates.fromTimeSincePeriapsis(sunGravity, 0.8882f, 30535.3711f, 28.1204f, 81.8517f, 91.2994f, 1253.74866f)));
                mapping.Add(Position.HeavenlyBodies.MapSatilite, new Plantoid(new Position.Size(5, 100), Gravity.of(2, 500), new Quaternion(-0.084f, -0.76f, -0.1f, 0.637f), 0.0048f, Position.HeavenlyBodies.Sun, KeplerCoordinates.fromTimeSincePeriapsis(sunGravity, 0.0002f, 26000, 10, 344.9661f, 270, 329.33386f)));
                
                return mapping;
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
        }

        public static void Destroy()
        {
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

            updateList();
        }

        public static void FixedUpdate()
        {
            if (_movingItems.Count > 0)
            {
                relocateMovingItems(_newState, _movingItems);
            }
            _movingItems.Clear();
            _newState.Clear();
        }

        private static void updateList()
        {
            if (update)
            {
                update = false;
                _movingItems.Clear();
                _newState.Clear();

                _movingItems = trackMovingItems();
                foreach (var body in _mapping.Keys)
                {
                    var state = updatePlanet(body, _newState);
                    if (state != null) {
                        _newState.Add(body, state);
                    }
                }
            }
        }

        private static List<Tuple<OWRigidbody, RelativeState>> trackMovingItems()
        {
            var sunStation = Position.getBody(Position.HeavenlyBodies.SunStation);
            var giantDeep = Position.getBody(Position.HeavenlyBodies.GiantsDeep);
            var probeCannon = Position.getBody(Position.HeavenlyBodies.ProbeCannon);
            var whiteHole = Position.getBody(Position.HeavenlyBodies.WhiteHole);

            List<Tuple<OWRigidbody, RelativeState>> bodies = new List<Tuple<OWRigidbody, RelativeState>>();
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.Player)));
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.Ship)));
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.Probe)));
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.ModelShip)));
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.NomaiProbe)));
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.NomaiEmberTwinShuttle)));
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.NomaiBrittleHollowShuttle)));

            foreach (var child in GameObject.FindObjectsOfType<OWRigidbody>())
            {
                var name = child?.gameObject?.name;
                if (name == null)
                {
                    continue;
                }
                if (sunStation != null)
                {
                    if (child.GetOrigParentBody() == sunStation
                            && (name.StartsWith("SS_Debris_Body")))
                    {
                        var surface = RelativeState.getSurfaceMovement(Position.HeavenlyBodies.SunStation, child);
                        bodies.Add(Tuple.Create(child, RelativeState.fromSurface(Position.HeavenlyBodies.SunStation, surface)));
                    }
                }
                if (probeCannon != null)
                {
                    if (child.GetOrigParentBody() == probeCannon
                        && (name.StartsWith("Debris_Body")
                            || name.StartsWith("FakeCannonMuzzle_Body")))
                    {
                        var surface = RelativeState.getSurfaceMovement(Position.HeavenlyBodies.ProbeCannon, child);
                        bodies.Add(Tuple.Create(child, RelativeState.fromSurface(Position.HeavenlyBodies.ProbeCannon, surface)));
                    }
                }
                if (giantDeep != null)
                {
                    if (child.GetOrigParentBody() == giantDeep
                        && (name.StartsWith("GabbroShip_Body")
                            || name.StartsWith("StatueIsland_Body")
                            || name.StartsWith("ConstructionYardIsland_Body")
                            || name.StartsWith("GabbroIsland_Body")
                            || name.StartsWith("QuantumIsland_Body")
                            || name.StartsWith("BrambleIsland_Body")))
                    {
                        bodies.Add(captureState(child, Position.HeavenlyBodies.GiantsDeep));
                    }
                }
                if (whiteHole != null)
                {
                    if (child.GetOrigParentBody() == whiteHole
                        && (name.StartsWith("WhiteholeStationSuperstructure_Body")))
                    {
                        bodies.Add(captureState(child, Position.HeavenlyBodies.WhiteHoleStation));
                    }
                }
            }

            return bodies;
        }

        private static void relocateMovingItems(Dictionary<Position.HeavenlyBodies, AbsoluteState> newStates, List<Tuple<OWRigidbody, RelativeState>> movingItems)
        {
            foreach(var movingItem in movingItems)
            {
                if (movingItem == null || movingItem.Item1 == null || movingItem.Item2 == null || !newStates.ContainsKey(movingItem.Item2.parent))
                {
                    continue;
                }

                var parentState = newStates[movingItem.Item2.parent];
                var parentGravity = Position.getGravity(movingItem.Item2.parent);
                if (parentState == null || parentGravity == null)
                {
                    continue;
                }
                movingItem.Item2.apply(movingItem.Item1, parentState, parentGravity);
            }
        }

        private static Tuple<OWRigidbody, RelativeState> captureState(OWRigidbody item, Position.HeavenlyBodies parent)
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

            return Tuple.Create(item, RelativeState.fromClosetInfluence(item, Position.HeavenlyBodies.Sun,
                Position.HeavenlyBodies.Player,
                Position.HeavenlyBodies.Probe,
                Position.HeavenlyBodies.Ship,
                Position.HeavenlyBodies.ModelShip,
                Position.HeavenlyBodies.NomaiProbe,
                Position.HeavenlyBodies.NomaiBrittleHollowShuttle,
                Position.HeavenlyBodies.NomaiEmberTwinShuttle,
                Position.HeavenlyBodies.TimberHearthProbe,
                Position.HeavenlyBodies.EyeOfTheUniverse,
                Position.HeavenlyBodies.EyeOfTheUniverse_Vessel));
        }

        private static Gravity getGravity(Position.HeavenlyBodies parent)
        {
            if (parent == Position.HeavenlyBodies.HourglassTwins)
            {
                var emberTwin = _mapping[Position.HeavenlyBodies.EmberTwin];
                var ashTwin = _mapping[Position.HeavenlyBodies.AshTwin];
                var exponent = (emberTwin.gravity.exponent + ashTwin.gravity.exponent) / 2f;
                var mass = (emberTwin.gravity.mass + ashTwin.gravity.mass) / 4f;

                return Gravity.of(exponent, mass);
            }
            else if (_mapping.ContainsKey(parent))
            {

                var parentMap = _mapping[parent];
                return Gravity.of(parentMap.gravity.exponent, parentMap.gravity.mass);
            }

            return null;
        }

        private static AbsoluteState updatePlanet(Position.HeavenlyBodies body, Dictionary<Position.HeavenlyBodies, AbsoluteState> newStates)
        {
            var owBody = Position.getBody(body);
            if (owBody == null || !_mapping.ContainsKey(body))
            {
                return null;
            }
            var planet = _mapping[body];

            updatePlanetGravity(planet, owBody);
            if (body == Position.HeavenlyBodies.QuantumMoon)
            {
                return null;
            }
            updatePlanetParent(planet.state.parent, owBody);

            var gravity = getGravity(planet.state.parent);
            AbsoluteState parentState = null;
            if (newStates.ContainsKey(planet.state.parent))
            {
                parentState = newStates[planet.state.parent];
            };
            return updatePlanetPosition(parentState, gravity, planet.state, owBody);
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

        private static void updatePlanetParent(Position.HeavenlyBodies parent, OWRigidbody owBody)
        {

            var owParent = Position.getBody(parent);
            if (owParent != null)
            {
                if (owParent.transform != null)
                {
                    if (owBody?.transform != null)
                    {
                        owBody.transform.parent = owParent.transform;
                    }
                    owBody.SetValue("_origParent", owParent.transform);
                    owBody.SetValue("_origParentBody", owParent);
                }
            }
            else if (parent == Position.HeavenlyBodies.None)
            {
                if (owBody?.transform != null)
                {
                    owBody.transform.parent = null;
                }
                owBody.SetValue("_origParent", null);
                owBody.SetValue("_origParentBody", null);
            }
        }

        private static AbsoluteState updatePlanetPosition(AbsoluteState parentState, Gravity gravity, RelativeState relativeState, OWRigidbody owBody)
        {
            return relativeState.apply(owBody, parentState, gravity);
        }

        private static bool onOrbitLineUpdate(ref OrbitLine __instance)
        {
            var _astroObject = __instance.GetValue<AstroObject>("_astroObject");
            var _lineRenderer = __instance.GetValue<LineRenderer>("_lineRenderer");
            _lineRenderer.startColor = Color.clear;
            _lineRenderer.endColor = Color.clear;

            AstroObject parentAstro = _astroObject != null ? _astroObject.GetPrimaryBody() : (AstroObject)null;
            if (parentAstro == null)
            {
                return false;
            }

            var body = Position.find(_astroObject);
            Plantoid planet;
            if (!_mapping.TryGetValue(body, out planet))
            {
                return false;
            }

            var parent = planet.state.parent;
            var owBody = Position.getBody(body);
            var kepler = Position.getKepler(parent, owBody);
            if (kepler == null && !kepler.isOrbit())
            {
                return false;
            }

            var _numVerts = __instance.GetValue<int>("_numVerts");
            var _verts = new Vector3[_numVerts];

            var parentGravity = Position.getGravity(parent);
            var semiAxis = new Vector2(kepler.semiMajorRadius, kepler.semiMinorRadius);
            var angle = KeplerCoordinates.shiftTimeSincePeriapsis(parentGravity, kepler, Time.timeSinceLevelLoad).esccentricAnomaly;
            var increment = Circle.getPercentageAngle(1f / (float)(_numVerts - 1));
            for (int index = 0; index < _numVerts; ++index)
            {
                var vert = Ellipse.fromPolar((angle + 180) - (index * increment), semiAxis);
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
            _lineRenderer.startColor = new Color(_color.r, _color.g, _color.b, num2 * num2);

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

            return false;
        }

        private static bool onInitialVelocityStart(ref InitialVelocity __instance)
        {
            var owBody = __instance.GetValue<OWRigidbody>("_owRigidbody");
            var body = Position.find(owBody);
            if (_mapping.ContainsKey(body))
            {
                update = true;
            }

            return false;
        }

        private static bool onMatchInitialMotionStart(ref MatchInitialMotion __instance)
        {
            var owBody = __instance.GetValue<OWRigidbody>("_owRigidbody");
            var body = Position.find(owBody);
            if (_mapping.ContainsKey(body))
            {
                update = true;
            }

            return false;
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

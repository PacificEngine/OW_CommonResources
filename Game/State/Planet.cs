using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game.Display;
using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Geometry;
using UnityEngine;
using static PacificEngine.OW_CommonResources.Game.Resource.Position;

namespace PacificEngine.OW_CommonResources.Game.State
{
    // TODO: Handle Ship, Player, Probe, ModelShip, And nomai Shuttle
    // TODO: Handle Probe Cannon Debris and Probe
    // TODO: Handle Giant's Deep Islands
    public static class Planet
    {
        private const string classId = "PacificEngine.OW_CommonResources.Game.State.Planet";

        public class Plantoid
        {
            public float falloffExponent { get; }
            public float mass { get; }
            public Quaternion orientation { get; }
            public float rotationalSpeed { get; }
            public HeavenlyBodies parent { get; }
            public float time { get; }
            public Vector3? startPosition { get; }
            public Vector3? startVelocity { get; }
            public Orbit.KeplerCoordinates orbit { get; }

            public Plantoid(float falloffExponent, float mass, Quaternion orientation, float rotationalSpeed, HeavenlyBodies parent, float time, Vector3? position, Vector3? velocity, Orbit.KeplerCoordinates orbit)
            {
                this.falloffExponent = falloffExponent;
                this.mass = mass;
                this.orientation = orientation;
                this.rotationalSpeed = rotationalSpeed;
                this.parent = parent;
                this.time = time;
                this.startPosition = position;
                this.startVelocity = velocity;
                this.orbit = orbit;
            }

            public override string ToString()
            {
                var position = startPosition == null ? "null" : DisplayConsole.logVector(startPosition.Value);
                var velocity = startVelocity == null ? "null" : DisplayConsole.logVector(startVelocity.Value);
                var orbit = (this.orbit?.ToString() ?? "");
                return $"({Math.Round(falloffExponent,1).ToString("G1")}, {Math.Round(mass, 4).ToString("G4")}, {DisplayConsole.logQuaternion(orientation)}, {Math.Round(rotationalSpeed, 4).ToString("G4")}, {parent}, {position}, {velocity}, {orbit})";
            }

            public override bool Equals(System.Object other)
            {
                if (other != null && other is Plantoid)
                {
                    var obj = other as Plantoid;
                    return falloffExponent == obj.falloffExponent
                        && mass == obj.mass
                        && orientation == obj.orientation
                        && rotationalSpeed == obj.rotationalSpeed
                        && parent == obj.parent
                        && startPosition == obj.startPosition
                        && startVelocity == obj.startVelocity
                        && orbit == obj.orbit;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (falloffExponent.GetHashCode() * 4)
                    + (mass.GetHashCode() * 16)
                    + (orientation.GetHashCode() * 64)
                    + (rotationalSpeed.GetHashCode() * 256)
                    + (parent.GetHashCode() * 1024)
                    + ((startPosition?.GetHashCode() ?? 0) * 4096)
                    + ((startVelocity?.GetHashCode() ?? 0) * 16384)
                    + (orbit.GetHashCode() * 64884);
            }
        }

        private static float lastUpdate = 0f;
        private static List<string> debugIds = new List<string>();
        public static bool debugPlanetPosition { get; set; } = false;

        private static Dictionary<HeavenlyBodies, Tuple<InitialMotion, Vector3, Vector3, Quaternion, Vector3, GravityVolume>> dict = new Dictionary<HeavenlyBodies, Tuple<InitialMotion, Vector3, Vector3, Quaternion, Vector3, GravityVolume>>();
        private static Dictionary<Position.HeavenlyBodies, Plantoid> _mapping = defaultMapping;
        private static bool update = false;


        public static Dictionary<Position.HeavenlyBodies, Plantoid> mapping
        {
            get
            {
                Dictionary<Position.HeavenlyBodies, Plantoid> mapping = new Dictionary<Position.HeavenlyBodies, Plantoid>();
                foreach (HeavenlyBodies body in _mapping.Keys)
                {
                    var owBody = Position.getBody(body);
                    if (owBody != null)
                    {
                        var parent = _mapping[body].parent;

                        var exponent = owBody?.GetAttachedGravityVolume()?.GetValue<float>("_falloffExponent") ?? 1;
                        var mass = owBody?.GetAttachedGravityVolume()?.GetValue<float>("_gravitationalMass") ?? ((owBody?.GetMass() ?? 0f) * 1000f);
                        var position = Position.getRelativePosition(parent, owBody);
                        var velocity = Position.getRelativeVelocity(parent, owBody);

                        var kepler = getKepler(parent, owBody);
                        kepler = (kepler == null || !kepler.isOrbit()) ? null : kepler;
                        var time = Time.timeSinceLevelLoad;

                        mapping.Add(body, new Plantoid(exponent, mass, owBody?.GetRotation() ?? Quaternion.identity, owBody?.GetAngularVelocity().magnitude ?? 0f, parent, time, position, velocity, kepler));
                    }
                    else
                    {
                        mapping.Add(body, new Plantoid(_mapping[body].falloffExponent, _mapping[body].mass, _mapping[body].orientation, _mapping[body].rotationalSpeed, _mapping[body].parent, _mapping[body].time, _mapping[body].startPosition, _mapping[body].startVelocity, _mapping[body].orbit));
                    }
                }

                return mapping;
            }
            set
            {
                _mapping = value;
                updateList();
            }
        }

        public static Dictionary<Position.HeavenlyBodies, Plantoid> defaultMapping
        {
            get
            {
                var mapping = new Dictionary<Position.HeavenlyBodies, Plantoid>();
                mapping.Add(HeavenlyBodies.Sun, new Plantoid(2, 4E+11f, new Quaternion(0, 0, 0, 1), 0f, HeavenlyBodies.None, 0f, Vector3.zero, Vector3.zero, null));
                mapping.Add(HeavenlyBodies.SunStation, new Plantoid(1, 1000, new Quaternion(0.5f, 0.5f, -0.5f, -0.5f), 0.1817f, HeavenlyBodies.Sun, 0f, new Vector3(0, 0, -2296), new Vector3(417.4f, 0, 0), new Orbit.KeplerCoordinates(0.0002f, 2295.80151f, 90, 90, 0, 17.27803f)));
                mapping.Add(HeavenlyBodies.HourglassTwins, new Plantoid(1, 1000, new Quaternion(0, -0.9f, 0, 0.5f), 0f, HeavenlyBodies.Sun, 0f, new Vector3(-2867.9f, 0, 4095.8f), new Vector3(-231.7f, 0, -162.2f), new Orbit.KeplerCoordinates(0.0004f, 4999.06934f, 90, 334.142f, 0, 46.52228f)));
                mapping.Add(HeavenlyBodies.AshTwin, new Plantoid(1, 1600000, new Quaternion(0, 1.0f, 0, 0.3f), 0.07f, HeavenlyBodies.HourglassTwins, 0f, new Vector3(204.8f, 0, 143.4f), new Vector3(16.2f, 0, -23.2f), new Orbit.KeplerCoordinates(0, 250, 90, 233.8092f, 180, 41.86327f)));
                mapping.Add(HeavenlyBodies.EmberTwin, new Plantoid(1, 1600000, new Quaternion(0, 0.9f, 0, 0.5f), 0.05f, HeavenlyBodies.HourglassTwins, 0f, new Vector3(-204.8f, 0, -143.4f), new Vector3(-16.2f, 0, 23.2f), new Orbit.KeplerCoordinates(0.0004f, 250.119278f, 90, 53.8093f, 180, 41.86327f)));
                mapping.Add(HeavenlyBodies.TimberHearth, new Plantoid(1, 3000000, new Quaternion(0, 1, 0, 0.1f), 0.01f, HeavenlyBodies.Sun, 0f, new Vector3(1492.2f, 0, -8462.5f), new Vector3(212.5f, 0, 37.5f), new Orbit.KeplerCoordinates(0.0004f, 8594.43066f, 90, 320.244f, 0, 222.33414f)));
                mapping.Add(HeavenlyBodies.TimberHearthProbe, new Plantoid(1, 10, new Quaternion(0.7f, -0.7f, 0.1f, -0.1f), 0f, HeavenlyBodies.TimberHearth, 0f, new Vector3(-344.8f, 0, -60.8f), new Vector3(9.5f, 0, -53.9f), new Orbit.KeplerCoordinates(0.0008f, 349.854706f, 90, 72.9561f, 0, 13.03971f)));
                mapping.Add(HeavenlyBodies.Attlerock, new Plantoid(2, 50000000, new Quaternion(0, -0.6f, 0, -0.8f), 0.0609f, HeavenlyBodies.TimberHearth, 0f, new Vector3(886.3f, 0, 156.3f), new Vector3(9.5f, 0, -53.9f), new Orbit.KeplerCoordinates(0.0008f, 899.295593f, 90, 265.2355f, 0, 29.99792f)));
                mapping.Add(HeavenlyBodies.BrittleHollow, new Plantoid(1, 3000000, new Quaternion(0, 0.6f, 0, -0.8f), 0.02f, HeavenlyBodies.Sun, 0f, new Vector3(11513.3f, 0, -2030.1f), new Vector3(32.1f, 0, 182.2f), new Orbit.KeplerCoordinates(0.0002f, 11693.7646f, 90, 20.21f, 0, 363.92078f)));
                mapping.Add(HeavenlyBodies.HollowLantern, new Plantoid(1, 910000, new Quaternion(-0.5f, 0.5f, -0.5f, -0.5f), 0.2f, HeavenlyBodies.BrittleHollow, 0f, new Vector3(984.8f, 0, -173.6f), new Vector3(9.5f, 0, 53.9f), new Orbit.KeplerCoordinates(0.0008f, 999.227661f, 90, 122.3145f, 0, 72.51768f)));
                mapping.Add(HeavenlyBodies.GiantsDeep, new Plantoid(1, 21780000, new Quaternion(0, 0.1f, 0, -1.0f), 0f, HeavenlyBodies.Sun, 0f, new Vector3(3421.7f, 0, -16098.0f), new Vector3(152.5f, 0, 32.4f), new Orbit.KeplerCoordinates(0.0003f, 16456.3711f, 90, 151.9485f, 0, 239.52866f)));
                mapping.Add(HeavenlyBodies.ProbeCannon, new Plantoid(1, 1000, new Quaternion(-0.3f, 0.5f, 0.4f, 0.7f), 0f, HeavenlyBodies.GiantsDeep, 0f, new Vector3(-1006.4f, 0, 653.6f), new Vector3(80.4f, 0, 123.8f), new Orbit.KeplerCoordinates(0.0002f, 1200.30615f, 90, 351.1031f, 180, 5.94489f)));
                mapping.Add(HeavenlyBodies.DarkBramble, new Plantoid(1, 3250000, new Quaternion(0, 1, 0, 0.1f), 0f, HeavenlyBodies.Sun, 0f, new Vector3(-3473.0f, 0, 19696.2f), new Vector3(-139.3f, 0, -24.6f), new Orbit.KeplerCoordinates(0.0005f, 20007.2539f, 90, 135.9328f, 0, 800.35431f)));
                mapping.Add(HeavenlyBodies.Interloper, new Plantoid(1, 550000, new Quaternion(0, 1, 0, 0.1f), 0.0034f, HeavenlyBodies.Sun, 0f, new Vector3(-24100, 0, 0), new Vector3(0, 0, 54.8f), new Orbit.KeplerCoordinates(0.8191f, 13248.3867f, 90, 180, 180, 239.51747f)));
                mapping.Add(HeavenlyBodies.Stranger, new Plantoid(1, 300000000, new Quaternion(-0.4f, -0.9f, 0, -0.2f), 0.05f, HeavenlyBodies.Sun, 0f, new Vector3(8168.2f, 8400f, 2049.5f), Vector3.zero, null)); ;
                mapping.Add(HeavenlyBodies.BackerSatilite, new Plantoid(1, 100, new Quaternion(0, 0, 0, 1), 0f, HeavenlyBodies.Sun, 0f, new Vector3(41999.8f, 5001.7f, -22499.9f), new Vector3(-46.9f, 28.1f, 24.7f), new Orbit.KeplerCoordinates(0.8884588f, 30535.38f, 28.1183f, 81.81603f, 91.35296f, 1253.788f)));
                mapping.Add(HeavenlyBodies.MapSatilite, new Plantoid(1, 500, new Quaternion(-0.1f, -0.8f, -0.1f, 0.6f), 0.0048f, HeavenlyBodies.Sun, 0f, new Vector3(24732.5f, -6729.5f, 4361), new Vector3(31.6f, 119.8f, 5.6f), new Orbit.KeplerCoordinates(0.0003f, 25992.3047f, 10.0033f, 241.6748f, 270.071f, 706.70221f)));
                
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
                        console.setElement(id + ".1", $" {map.Key.ToString()}: {Math.Round(map.Value.falloffExponent, 1).ToString("G1")}, {Math.Round(map.Value.mass, 4).ToString("G4")}, {DisplayConsole.logQuaternion(map.Value.orientation)}, {Math.Round(map.Value.rotationalSpeed, 4).ToString("G4")}, {map.Value.parent.ToString()}", index + 0.0001f);
                        if (map.Value.orbit != null && map.Value.orbit.isOrbit())
                        {
                            console.setElement(id + ".2", $"{map.Value.time.ToString()}, {map.Value.orbit.ToString()}", index + 0.0002f);
                        }
                        else
                        {
                            console.setElement(id + ".2", $"{map.Value.time.ToString()}, {DisplayConsole.logVector(map.Value.startPosition)}, {DisplayConsole.logVector(map.Value.startVelocity)}", index + 0.0002f);
                        }                        

                        index += 0.0005f;
                    }
                }
            }

            updateList();
        }

        private static void updateList()
        {
            if (update)
            {
                update = false;
                foreach (var body in _mapping.Keys)
                {
                    updatePlanet(body);
                }
            }
        }

        private static void updatePlanet(HeavenlyBodies body)
        {
            if (!_mapping.ContainsKey(body))
            {
                return;
            }

            var planet = _mapping[body];
            var parent = planet.parent;

            var position = planet.startPosition ?? Vector3.zero;
            var velocity = planet.startVelocity ?? Vector3.zero;

            if (_mapping.ContainsKey(parent))
            {
                var parentMap = _mapping[parent];
                if (planet.orbit != null && planet.orbit.isOrbit())
                { 
                    float exponent;
                    float mass;
                    if (parent == HeavenlyBodies.HourglassTwins)
                    {
                        var emberTwin = _mapping[HeavenlyBodies.EmberTwin];
                        var ashTwin = _mapping[HeavenlyBodies.AshTwin];
                        exponent = (emberTwin.falloffExponent + ashTwin.falloffExponent) / 2f;
                        mass = (emberTwin.mass + ashTwin.mass) / 4f;
                    }
                    else
                    {
                        exponent = parentMap.falloffExponent;
                        mass = parentMap.mass;
                    }

                    var result = Orbit.toCartesian(GravityVolume.GRAVITATIONAL_CONSTANT, mass, exponent, Time.timeSinceLevelLoad, planet.orbit);
                    position = result.Item1;
                    velocity = result.Item2;
                }                
            }

            updatePlanet(body, planet.parent, planet.mass, planet.falloffExponent, position, velocity, planet.orientation, planet.rotationalSpeed);
        }

        private static void updatePlanet(HeavenlyBodies body, HeavenlyBodies parent, float mass, float falloffExponent, Vector3 position, Vector3 velocity, Quaternion orientation, float rotationalSpeed)
        {
            var owBody = Position.getBody(body);
            if (owBody == null)
            {
                return;
            }

            // Adjust Mass
            var gravity = owBody?.GetAttachedGravityVolume();
            if (gravity != null)
            {
                var _upperSurfaceRadius = gravity.GetValue<float>("_upperSurfaceRadius");
                var _surfaceAcceleration = (GravityVolume.GRAVITATIONAL_CONSTANT * mass) / Mathf.Pow(_upperSurfaceRadius, falloffExponent);

                gravity.SetValue("_falloffExponent", falloffExponent);
                gravity.SetValue("_gravitationalMass", mass);
                gravity.SetValue("_surfaceAcceleration", _surfaceAcceleration);
            }
            owBody.SetMass(mass);

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
                    position += owParent.transform.position;
                }
                else
                {
                    position += owParent.GetPosition();
                }
                velocity += owParent.GetVelocity();
            }
            else if (parent == HeavenlyBodies.None)
            {
                if (owBody?.transform != null)
                {
                    owBody.transform.parent = null;
                }
                owBody.SetValue("_origParent", null);
                owBody.SetValue("_origParentBody", null);
                position += Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero;
                velocity += Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero;
            }
            else
            {
                position += Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero;
                velocity += Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero;
            }
            var angularVelocity = orientation * (Vector3.up * rotationalSpeed);

            owBody.SetPosition(position);
            owBody.SetVelocity(velocity);
            owBody.SetRotation(orientation);
            owBody.SetAngularVelocity(angularVelocity);

            if (owBody?.transform != null)
            {
                owBody.transform.position = position;
                owBody.transform.rotation = orientation;
            }

            owBody.SetValue("_lastPosition", position);
            owBody.SetValue("_currentVelocity", velocity);
            owBody.SetValue("_lastVelocity", velocity);
            owBody.SetValue("_currentAngularVelocity", angularVelocity);
            owBody.SetValue("_lastAngularVelocity", angularVelocity);
            owBody.SetValue("_currentAccel", Vector3.zero);
            owBody.SetValue("_lastAccel", Vector3.zero);
        }

        private static bool onOrbitLineUpdate(ref OrbitLine __instance)
        {
            var _astroObject = __instance.GetValue<AstroObject>("_astroObject");
            AstroObject parentAstro = _astroObject != null ? _astroObject.GetPrimaryBody() : (AstroObject)null;
            if (parentAstro == null)
            {
                return true;
            }

            var body = find(_astroObject);
            Plantoid planet;
            if (!_mapping.TryGetValue(body, out planet))
            {
                return true;
            }

            var parent = planet.parent;
            var owBody = getBody(body);
            var kepler = Position.getKepler(parent, owBody);
            if (kepler == null && !kepler.isOrbit())
            {
                return true;
            }

            var _numVerts = __instance.GetValue<int>("_numVerts");
            var _lineRenderer = __instance.GetValue<LineRenderer>("_lineRenderer");
            var _verts = new Vector3[_numVerts];

            var parentMass = Position.getMass(parent);
            var semiAxis = new Vector2(kepler.semiMajorRadius, kepler.semiMinorRadius);
            var angle = Orbit.getEsscentricAnomalyAngle(GravityVolume.GRAVITATIONAL_CONSTANT, parentMass.Item2, parentMass.Item1, Time.timeSinceLevelLoad, kepler);
            var increment = Circle.getPercentageAngle(1f / (float)(_numVerts - 1));
            for (int index = 0; index < _numVerts; ++index)
            {
                var vert = Ellipse.fromPolar((angle + 180) - (index * increment), semiAxis);
                _verts[index] = new Vector3(vert.y, 0, vert.x);
            }
            _lineRenderer.SetPositions(_verts);

            var periapsis = Orbit.getPeriapsis(GravityVolume.GRAVITATIONAL_CONSTANT, parentMass.Item2, parentMass.Item1, kepler);
            var semiMinorDecending = Orbit.getSemiMinorDecending(GravityVolume.GRAVITATIONAL_CONSTANT, parentMass.Item2, parentMass.Item1, kepler);
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
                return false;
            }

            return true;
        }
    }
}

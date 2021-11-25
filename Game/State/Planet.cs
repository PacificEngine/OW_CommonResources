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
                var orbit = (this.orbit?.ToString() ?? "");
                return $"({Math.Round(falloffExponent,1).ToString("G1")}, {Math.Round(mass, 4).ToString("G4")}, {DisplayConsole.logQuaternion(orientation)}, {Math.Round(rotationalSpeed, 4).ToString("G4")}, {parent}, {DisplayConsole.logVector(startPosition.Value)}, {DisplayConsole.logVector(startVelocity.Value)}, {orbit})";
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
                        var time = kepler == null ? Time.timeSinceLevelLoad : 0f;

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

                // TODO: Update game
            }
        }

        public static Dictionary<Position.HeavenlyBodies, Plantoid> defaultMapping
        {
            get
            {
                var mapping = new Dictionary<Position.HeavenlyBodies, Plantoid>();
                mapping.Add(HeavenlyBodies.Sun, new Plantoid(2, 4E+11f, new Quaternion(0, 0, 0, 1), 0f, HeavenlyBodies.None, 0f, Vector3.zero, Vector3.zero, null));
                mapping.Add(HeavenlyBodies.SunStation, new Plantoid(1, 1000, new Quaternion(0.5f, 0.5f, -0.5f, -0.5f), 0.1817f, HeavenlyBodies.Sun, 0f, new Vector3(0, 0, -2296), new Vector3(417.4f, 0, 0), new Orbit.KeplerCoordinates(0, 2296, 90f, 90f, 0, 17.25547f)));
                mapping.Add(HeavenlyBodies.HourglassTwins, new Plantoid(1, 1000, new Quaternion(0, -0.9f, 0, 0.5f), 0f, HeavenlyBodies.Sun, 0f, new Vector3(-2867.9f, 0, 4095.8f), new Vector3(-231.7f, 0, -162.2f), new Orbit.KeplerCoordinates(0, 5000.001f, 90, 305.2348f, 0, 55.46355f)));
                mapping.Add(HeavenlyBodies.AshTwin, new Plantoid(1, 1600000, new Quaternion(0, 1.0f, 0, 0.3f), 0.07f, HeavenlyBodies.HourglassTwins, 0f, new Vector3(204.8f, 0, 143.4f), new Vector3(16.2f, 0, -23.2f), new Orbit.KeplerCoordinates(0, 250, 90, 233.8092f, 180, 41.86327f)));
                mapping.Add(HeavenlyBodies.EmberTwin, new Plantoid(1, 1600000, new Quaternion(0, 0.9f, 0, 0.5f), 0.05f, HeavenlyBodies.HourglassTwins, 0f, new Vector3(-204.8f, 0, -143.4f), new Vector3(-16.2f, 0, 23.2f), new Orbit.KeplerCoordinates(0, 250, 90, 53.80923f, 180, 41.86327f)));
                mapping.Add(HeavenlyBodies.TimberHearth, new Plantoid(1, 3000000, new Quaternion(0, 1, 0, 0.1f), 0.01f, HeavenlyBodies.Sun, 0f, new Vector3(1492.2f, 0, -8462.5f), new Vector3(212.5f, 0, 37.5f), new Orbit.KeplerCoordinates(0, 8593.086f, 90, 95.02123f, 0, 125.1692f)));
                mapping.Add(HeavenlyBodies.TimberHearthProbe, new Plantoid(1, 10, new Quaternion(0.7f, -0.7f, 0.1f, -0.1f), 0f, HeavenlyBodies.TimberHearth, 0f, new Vector3(-344.8f, 0, -60.8f), new Vector3(9.5f, 0, -53.9f), new Orbit.KeplerCoordinates(0, 350.1f, 90, 72.95632f, 0, 13.03968f)));
                mapping.Add(HeavenlyBodies.Attlerock, new Plantoid(2, 50000000, new Quaternion(0, -0.6f, 0, -0.8f), 0.0609f, HeavenlyBodies.TimberHearth, 0f, new Vector3(886.3f, 0, 156.3f), new Vector3(9.5f, 0, -53.9f), new Orbit.KeplerCoordinates(0, 900.0001f, 90, 265.2352f, 0, 29.998f)));
                mapping.Add(HeavenlyBodies.BrittleHollow, new Plantoid(1, 3000000, new Quaternion(0, 0.6f, 0, -0.8f), 0.02f, HeavenlyBodies.Sun, 0f, new Vector3(11513.3f, 0, -2030.1f), new Vector3(32.1f, 0, 182.2f), new Orbit.KeplerCoordinates(0, 11690.89f, 90, 170.0452f, 0, 198.5069f)));
                mapping.Add(HeavenlyBodies.HollowLantern, new Plantoid(1, 910000, new Quaternion(-0.5f, 0.5f, -0.5f, -0.5f), 0.2f, HeavenlyBodies.BrittleHollow, 0f, new Vector3(984.8f, 0, -173.6f), new Vector3(9.5f, 0, 53.9f), new Orbit.KeplerCoordinates(0, 11690.89f, 90, 170.0452f, 0, 198.5069f)));
                mapping.Add(HeavenlyBodies.GiantsDeep, new Plantoid(1, 21780000, new Quaternion(0, 0.1f, 0, -1.0f), 0f, HeavenlyBodies.Sun, 0f, new Vector3(3421.7f, 0, -16098.0f), new Vector3(152.5f, 0, 32.4f), new Orbit.KeplerCoordinates(0, 16457.59f, 90, 101.9912f, 0, 331.6218f)));
                mapping.Add(HeavenlyBodies.ProbeCannon, new Plantoid(1, 1000, new Quaternion(-0.3f, 0.5f, 0.4f, 0.7f), 0f, HeavenlyBodies.GiantsDeep, 0f, new Vector3(-1006.4f, 0, 653.6f), new Vector3(80.4f, 0, 123.8f), new Orbit.KeplerCoordinates(0, 1200, 90, 351.1121f, 180, 5.943623f)));
                mapping.Add(HeavenlyBodies.DarkBramble, new Plantoid(1, 3250000, new Quaternion(0, 1, 0, 0.1f), 0f, HeavenlyBodies.Sun, 0f, new Vector3(-3473.0f, 0, 19696.2f), new Vector3(-139.3f, 0, -24.6f), new Orbit.KeplerCoordinates(0, 20000, 90, 280.0383f, 0, 444.1865f)));
                mapping.Add(HeavenlyBodies.Interloper, new Plantoid(1, 550000, new Quaternion(0, 1, 0, 0.1f), 0.0034f, HeavenlyBodies.Sun, 0f, new Vector3(-24100, 0, 0), new Vector3(0, 0, 54.8f), new Orbit.KeplerCoordinates(0.8193752f, 13246.31f, 90, 180, 180, 239.4758f)));
                mapping.Add(HeavenlyBodies.Stranger, new Plantoid(1, 300000000, new Quaternion(-0.4f, -0.9f, 0, -0.2f), 0.05f, HeavenlyBodies.Sun, 0f, new Vector3(8168.2f, 8400f, 2049.5f), Vector3.zero, null)); ;
                mapping.Add(HeavenlyBodies.BackerSatilite, new Plantoid(1, 100, new Quaternion(0, 0, 0, 1), 0f, HeavenlyBodies.Sun, 0f, new Vector3(41999.8f, 5001.7f, -22499.9f), new Vector3(-46.9f, 28.1f, 24.7f), new Orbit.KeplerCoordinates(0.8884588f, 30535.38f, 28.1183f, 81.81603f, 91.35296f, 1253.788f)));
                mapping.Add(HeavenlyBodies.MapSatilite, new Plantoid(1, 500, new Quaternion(-0.1f, -0.8f, -0.1f, 0.6f), 0.0048f, HeavenlyBodies.Sun, 0f, new Vector3(24732.5f, -6729.5f, 4361), new Vector3(31.6f, 119.8f, 5.6f), new Orbit.KeplerCoordinates(0, 26000, 10, 251.3132f, 270, 672.0341f)));
                
                return mapping;
            }
        }

        public static void Start()
        {
            //Helper.helper.HarmonyHelper.AddPostfix<InitialMotion>("Start", typeof(Planet), "onInitialMotionStart");
            //Helper.helper.HarmonyHelper.AddPostfix<GravityVolume>("Awake", typeof(Planet), "onGravityVolumeAwake");
            //Helper.helper.HarmonyHelper.AddPrefix<GravityVolume>("CalculateGravityMagnitude", typeof(Planet), "onGravityVolumeCalculateGravityMagnitude");
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
                        debugIds.Add(id);
                        console.setElement(id + ".1", $" {map.Key.ToString()}: {Math.Round(map.Value.falloffExponent, 1).ToString("G1")}, {Math.Round(map.Value.mass, 4).ToString("G4")}, {DisplayConsole.logQuaternion(map.Value.orientation)}, {Math.Round(map.Value.rotationalSpeed, 4).ToString("G4")}, {map.Value.parent.ToString()}", index + 0.0001f);
                        if (map.Value.orbit != null && map.Value.orbit.isOrbit())
                        {
                            console.setElement(id + ".2", $"{map.Value.time.ToString()}, {map.Value.orbit.ToString()}", index + 0.0002f);
                        }
                        else
                        {
                            console.setElement(id + ".2", $"{map.Value.time.ToString()}, {DisplayConsole.logVector(map.Value.startPosition)}, {DisplayConsole.logVector(map.Value.startVelocity)}", index + 0.0002f);
                        }
                        index += 0.0002f;
                    }
                }
            }
        }

        private static void onInitialMotionStart(ref InitialMotion __instance)
        {
            var body = __instance.GetValue<OWRigidbody>("_satelliteBody");
            var heavenlyBody = Position.find(body);

            if (heavenlyBody != HeavenlyBodies.None)
            {
                dict.Add(heavenlyBody, Tuple.Create(__instance, body.GetWorldCenterOfMass(), __instance.GetInitVelocity(), body.GetRotation(), __instance.GetInitAngularVelocity(), body?.GetAttachedGravityVolume()));
            }

            if (body?.GetAttachedGravityVolume() != null)
            {
                /*switch (heavenlyBody)
                {
                    case HeavenlyBodies.Sun:
                        body.GetAttachedGravityVolume().SetValue("_gravitationalMass", 4.00005E+12f);
                        body.SetMass(4.00005E+12f);
                        break;
                    case HeavenlyBodies.TimberHearth:
                        body.GetAttachedGravityVolume().SetValue("_gravitationalMass", 27000000000f);
                        body.SetMass(27000000000f);
                        break;
                }*/
            }
        }

        private static void onGravityVolumeAwake(ref GravityVolume __instance)
        {
            var _surfaceAcceleration = __instance.GetValue<float>("_surfaceAcceleration");
            var _upperSurfaceRadius = __instance.GetValue<float>("_upperSurfaceRadius");
            var _attachedBody = __instance.GetValue<OWRigidbody>("_attachedBody");

            var mass = (float)((double)_surfaceAcceleration * (double)Mathf.Pow(_upperSurfaceRadius, 2f) / (1.0 / 1000.0));
            //_surfaceAcceleration = mass / Mathf.Pow(_upperSurfaceRadius, 2f) / (1.0 / 1000.0));

            /*__instance.SetValue("_falloffExponent", 2f);
            __instance.SetValue("_gravitationalMass", mass);
            _attachedBody.SetMass(mass);*/
        }

            

        private static bool onGravityVolumeCalculateGravityMagnitude(ref GravityVolume __instance, ref float __result, ref float distanceToCenter)
        {
            var _cutoffAcceleration = __instance.GetValue<float>("_cutoffAcceleration");
            var _gravitationalMass = __instance.GetValue<float>("_gravitationalMass");
            var _surfaceAcceleration = __instance.GetValue<float>("_surfaceAcceleration");
            var _upperSurfaceRadius = __instance.GetValue<float>("_upperSurfaceRadius");
            var _lowerSurfaceRadius = __instance.GetValue<float>("_lowerSurfaceRadius");
            var _cutoffRadius = __instance.GetValue<float>("_cutoffRadius");
            var _falloffExponent = __instance.GetValue<float>("_falloffExponent");

            __result = _cutoffAcceleration;
            if ((double)distanceToCenter > (double)_upperSurfaceRadius)
                __result = (_gravitationalMass * GravityVolume.GRAVITATIONAL_CONSTANT) / Mathf.Pow(distanceToCenter, _falloffExponent);
            else if ((double)distanceToCenter > (double)_lowerSurfaceRadius)
                __result = (_gravitationalMass * GravityVolume.GRAVITATIONAL_CONSTANT) / Mathf.Pow(distanceToCenter, _falloffExponent);
            else if ((double)distanceToCenter > (double)_cutoffRadius)
                __result = Mathf.Lerp(_surfaceAcceleration, _cutoffAcceleration, (float)(1.0 - ((double)distanceToCenter - (double)_cutoffRadius) / ((double)_lowerSurfaceRadius - (double)_cutoffRadius)));
            return false;
        }
    }
}

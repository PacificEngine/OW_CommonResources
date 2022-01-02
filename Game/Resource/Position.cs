using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game.Display;
using PacificEngine.OW_CommonResources.Game.State;
using PacificEngine.OW_CommonResources.Geometry;
using PacificEngine.OW_CommonResources.Geometry.Orbits;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PacificEngine.OW_CommonResources.Game.Resource
{
    public static class Position
    {
        public class Size
        {
            public float size { get; }
            public float influence { get; }

            public Size(float size, float influence)
            {
                this.size = size;
                this.influence = influence;
            }

            public override string ToString()
            {
                return $"({Math.Round(size, 4).ToString("G4")}, {Math.Round(influence, 4).ToString("G4")})";
            }

            public override bool Equals(System.Object other)
            {
                if (other != null && other is Size)
                {
                    var obj = other as Size;
                    return size == obj.size
                        && influence == obj.influence;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (size.GetHashCode() * 4)
                    + (influence.GetHashCode() * 16);
            }
        }

        private const string classId = "PacificEngine.OW_CommonResources.Game.Resource.Position";

        private static float lastUpdate = 0f;
        private static List<string> debugIds = new List<string>();
        public static bool debugPlayerPosition { get; set; } = false;

        private static Dictionary<HeavenlyBody, Func<AstroObject>> _astroLookup = StandardAstroLookup;
        private static Dictionary<HeavenlyBody, Func<OWRigidbody>> _bodyLookup = StandardBodyLookup;
        private static Dictionary<HeavenlyBody, AstroObject> astros = new Dictionary<HeavenlyBody, AstroObject>();
        private static Dictionary<HeavenlyBody, OWRigidbody> bodies = new Dictionary<HeavenlyBody, OWRigidbody>();

        public static Dictionary<HeavenlyBody, Func<AstroObject>> StandardAstroLookup
        {
            get
            {
                var standardAstroLookup = new Dictionary<HeavenlyBody, Func<AstroObject>>();
                standardAstroLookup.Add(HeavenlyBodies.Sun, () => Locator.GetAstroObject(AstroObject.Name.Sun));
                standardAstroLookup.Add(HeavenlyBodies.SunStation, () => Locator.GetMinorAstroObject("Sun Station"));
                standardAstroLookup.Add(HeavenlyBodies.HourglassTwins, () => Locator.GetAstroObject(AstroObject.Name.TowerTwin)?.GetPrimaryBody());
                standardAstroLookup.Add(HeavenlyBodies.AshTwin, () => Locator.GetAstroObject(AstroObject.Name.TowerTwin));
                standardAstroLookup.Add(HeavenlyBodies.EmberTwin, () => Locator.GetAstroObject(AstroObject.Name.CaveTwin));
                standardAstroLookup.Add(HeavenlyBodies.TimberHearth, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth));
                standardAstroLookup.Add(HeavenlyBodies.TimberHearthProbe, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetSatellite());
                standardAstroLookup.Add(HeavenlyBodies.Attlerock, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetMoon());
                standardAstroLookup.Add(HeavenlyBodies.BrittleHollow, () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow));
                standardAstroLookup.Add(HeavenlyBodies.HollowLantern, () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow)?.GetMoon());
                standardAstroLookup.Add(HeavenlyBodies.GiantsDeep, () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep));
                standardAstroLookup.Add(HeavenlyBodies.ProbeCannon, () => Locator.GetAstroObject(AstroObject.Name.ProbeCannon));
                standardAstroLookup.Add(HeavenlyBodies.DarkBramble, () => Locator.GetAstroObject(AstroObject.Name.DarkBramble));
                standardAstroLookup.Add(HeavenlyBodies.InnerDarkBramble_Hub, () => Locator.GetMinorAstroObject("Hub Dimension"));
                standardAstroLookup.Add(HeavenlyBodies.InnerDarkBramble_EscapePod, () => Locator.GetMinorAstroObject("Escape Pod Dimension"));
                standardAstroLookup.Add(HeavenlyBodies.InnerDarkBramble_Nest, () => Locator.GetMinorAstroObject("Angler Nest Dimension"));
                standardAstroLookup.Add(HeavenlyBodies.InnerDarkBramble_Feldspar, () => Locator.GetMinorAstroObject("Pioneer Dimension"));
                standardAstroLookup.Add(HeavenlyBodies.InnerDarkBramble_Gutter, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.ExitOnly == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName()).GetAttachedOWRigidbody().GetComponent<AstroObject>());
                standardAstroLookup.Add(HeavenlyBodies.InnerDarkBramble_Vessel, () => Locator.GetMinorAstroObject("Vessel Dimension"));
                standardAstroLookup.Add(HeavenlyBodies.InnerDarkBramble_Maze, () => Locator.GetMinorAstroObject("Cluster Dimension"));
                standardAstroLookup.Add(HeavenlyBodies.InnerDarkBramble_SmallNest, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.SmallNest == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName()).GetAttachedOWRigidbody().GetComponent<AstroObject>());
                standardAstroLookup.Add(HeavenlyBodies.InnerDarkBramble_Secret, () => Locator.GetMinorAstroObject("Elsinore Dimension"));
                standardAstroLookup.Add(HeavenlyBodies.Interloper, () => Locator.GetAstroObject(AstroObject.Name.Comet));
                standardAstroLookup.Add(HeavenlyBodies.WhiteHole, () => Locator.GetAstroObject(AstroObject.Name.WhiteHole));
                standardAstroLookup.Add(HeavenlyBodies.WhiteHoleStation, () => Locator.GetAstroObject(AstroObject.Name.WhiteHoleTarget));
                standardAstroLookup.Add(HeavenlyBodies.Stranger, () => Locator.GetAstroObject(AstroObject.Name.RingWorld));
                standardAstroLookup.Add(HeavenlyBodies.DreamWorld, () => Locator.GetAstroObject(AstroObject.Name.DreamWorld));
                standardAstroLookup.Add(HeavenlyBodies.QuantumMoon, () => Locator.GetAstroObject(AstroObject.Name.QuantumMoon));
                standardAstroLookup.Add(HeavenlyBodies.SatiliteBacker, () => Locator.GetMinorAstroObject("Backer's Satellite"));
                standardAstroLookup.Add(HeavenlyBodies.SatiliteMapping, () => Helper.getSector(Sector.Name.Unnamed)?.Find(body => "Sector_HearthianMapSatellite".Equals(body?.gameObject?.name))?.GetAttachedOWRigidbody()?.GetComponent<AstroObject>());
                standardAstroLookup.Add(HeavenlyBodies.EyeOfTheUniverse, () => Helper.getSector(Sector.Name.EyeOfTheUniverse)?.Find(body => true)?.GetAttachedOWRigidbody()?.GetComponent<AstroObject>());
                standardAstroLookup.Add(HeavenlyBodies.EyeOfTheUniverse_Vessel, () => Helper.getSector(Sector.Name.Vessel)?.Find(body => Sector.Name.EyeOfTheUniverse == body.GetRootSector().GetName())?.GetAttachedOWRigidbody()?.GetComponent<AstroObject>());

                return standardAstroLookup;
            }
        }

        public static Dictionary<HeavenlyBody, Func<OWRigidbody>> StandardBodyLookup
        {
            get
            {
                var standardBodyLookup = new Dictionary<HeavenlyBody, Func<OWRigidbody>>();

                standardBodyLookup.Add(HeavenlyBodies.Player, () => Locator.GetPlayerBody());
                standardBodyLookup.Add(HeavenlyBodies.Ship, () => Locator.GetShipBody());
                standardBodyLookup.Add(HeavenlyBodies.Probe, () => Locator.GetProbe()?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.ModelShip, () => GameObject.Find("ModelRocket_Body")?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.Sun, () => Locator.GetAstroObject(AstroObject.Name.Sun)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.SunStation, () => Locator.GetWarpReceiver(NomaiWarpPlatform.Frequency.SunStation)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.HourglassTwins, () => Locator.GetAstroObject(AstroObject.Name.TowerTwin)?.GetPrimaryBody()?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.AshTwin, () => Locator.GetAstroObject(AstroObject.Name.TowerTwin)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.EmberTwin, () => Locator.GetAstroObject(AstroObject.Name.CaveTwin)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.TimberHearth, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.TimberHearthProbe, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetSatellite()?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.Attlerock, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetMoon()?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.BrittleHollow, () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.HollowLantern, () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow)?.GetMoon()?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.GiantsDeep, () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.ProbeCannon, () => Locator.GetAstroObject(AstroObject.Name.ProbeCannon)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.NomaiProbe, () => Locator.GetAstroObject(AstroObject.Name.ProbeCannon)?.GetComponent<OrbitalProbeLaunchController>()?.GetValue<OWRigidbody>("_probeBody"));
                standardBodyLookup.Add(HeavenlyBodies.NomaiEmberTwinShuttle, () => Locator.GetNomaiShuttle(NomaiShuttleController.ShuttleID.HourglassShuttle)?.GetOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.NomaiBrittleHollowShuttle, () => Locator.GetNomaiShuttle(NomaiShuttleController.ShuttleID.BrittleHollowShuttle)?.GetOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.DarkBramble, () => Locator.GetAstroObject(AstroObject.Name.DarkBramble)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Hub, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.Hub == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.InnerDarkBramble_EscapePod, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.EscapePod == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Nest, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.AnglerNest == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Feldspar, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.Pioneer == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Gutter, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.ExitOnly == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Vessel, () => Helper.getSector(Sector.Name.VesselDimension)?.Find(body => OuterFogWarpVolume.Name.Vessel == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Maze, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.Cluster == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.InnerDarkBramble_SmallNest, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.SmallNest == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Secret, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => body?.GetComponentInChildren<SecretFogWarpVolume>() != null)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.Interloper, () => Locator.GetAstroObject(AstroObject.Name.Comet)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.WhiteHole, () => Locator.GetAstroObject(AstroObject.Name.WhiteHole)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.WhiteHoleStation, () => Locator.GetAstroObject(AstroObject.Name.WhiteHoleTarget)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.Stranger, () => Locator.GetAstroObject(AstroObject.Name.RingWorld)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.DreamWorld, () => Locator.GetAstroObject(AstroObject.Name.DreamWorld)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.QuantumMoon, () => Locator.GetAstroObject(AstroObject.Name.QuantumMoon)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.SatiliteBacker, () => Locator.GetMinorAstroObject("Backer's Satellite")?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.SatiliteMapping, () => Helper.getSector(Sector.Name.Unnamed)?.Find(body => "Sector_HearthianMapSatellite".Equals(body?.gameObject?.name))?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.EyeOfTheUniverse, () => Helper.getSector(Sector.Name.EyeOfTheUniverse)?.Find(body => true)?.GetAttachedOWRigidbody());
                standardBodyLookup.Add(HeavenlyBodies.EyeOfTheUniverse_Vessel, () => Helper.getSector(Sector.Name.Vessel)?.Find(body => Sector.Name.EyeOfTheUniverse == body.GetRootSector().GetName())?.GetAttachedOWRigidbody());

                return standardBodyLookup;
            }
        }

        public static Dictionary<HeavenlyBody, Func<AstroObject>> AstroLookup
        {
            get
            {
                var value = new Dictionary<HeavenlyBody, Func<AstroObject>>();
                foreach (var val in _astroLookup)
                {
                    value.Add(val.Key, val.Value);
                }
                return value;
            }
            set
            {
                astros.Clear();
                _astroLookup.Clear();
                foreach (var val in value)
                {
                    _astroLookup.Add(val.Key, val.Value);
                }
            }
        }

        public static Dictionary<HeavenlyBody, Func<OWRigidbody>> BodyLookup
        {
            get
            {
                var value = new Dictionary<HeavenlyBody, Func<OWRigidbody>>();
                foreach (var val in _bodyLookup)
                {
                    value.Add(val.Key, val.Value);
                }
                return value;
            }
            set
            {
                bodies.Clear();
                _bodyLookup.Clear();
                foreach (var val in value)
                {
                    _bodyLookup.Add(val.Key, val.Value);
                }
            }
        }

        public static void Start()
        {
        }

        public static void Awake()
        {
            astros.Clear();
            bodies.Clear();
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

                if (debugPlayerPosition && Locator.GetPlayerBody())
                {
                    listValue("Player", "Player", 10f, Locator.GetPlayerBody());
                    listValue("Ship", "Ship", 10.1f, Locator.GetShipBody());
                    listValue("Probe", "Probe", 10.2f, Locator.GetProbe()?.GetAttachedOWRigidbody());
                }
            }
        }

        public static void FixedUpdate()
        {
        }

        private static void listValue(string id, string name, float index, OWRigidbody comparison)
        {
            if (comparison)
            {
                var absoluteState = PositionState.fromCurrentState(comparison);
                var parent = getClosetInfluence(absoluteState.position, getAstros(), new HeavenlyBody[0]);

                var relativeState = RelativeState.getSurfaceMovement(parent[0].Item1, comparison);
                listValue(id, name, index, parent[0].Item1, relativeState?.position ?? Vector3.zero, relativeState?.velocity ?? Vector3.zero);
            }
            else
            {
                listValue(id, name, index, null, Vector3.zero, Vector3.zero);
            }
        }

        private static void listValue(string id, string name, float index, HeavenlyBody body, Vector3 position, Vector3 velocity)
        {

            var console = DisplayConsole.getConsole(ConsoleLocation.BottomRight);
            if (body != null && body != HeavenlyBody.None)
            {
                debugIds.Add(classId + "." + id + ".Parent");
                debugIds.Add(classId + "." + id + ".Position");
                debugIds.Add(classId + "." + id + ".Velocity");
                console.setElement(classId + "." + id + ".Parent", $"{name} Parent: {body}", index + 0.01f);
                console.setElement(classId + "." + id + ".Position", $"{name} Position: {DisplayConsole.logVector(position)}", index + 0.02f);
                console.setElement(classId + "." + id + ".Velocity", $"{name} Velocity: {DisplayConsole.logVector(velocity)}", index + 0.03f);
            }
        }

        public static HeavenlyBody find(OWRigidbody body)
        {
            if (body == null)
            {
                return HeavenlyBody.None;
            }

            foreach (HeavenlyBody pp in HeavenlyBody.GetValues())
            {
                if (getBody(pp) == body)
                {
                    return pp;
                }
            }

            return HeavenlyBody.None;
        }

        public static HeavenlyBody find(AstroObject body)
        {
            if (body == null)
            {
                return HeavenlyBody.None;
            }

            foreach (HeavenlyBody pp in HeavenlyBody.GetValues())
            {
                if (getAstro(pp) == body)
                {
                    return pp;
                }
            }

            return HeavenlyBody.None;
        }

        private static AstroObject lookupAstro(HeavenlyBody value)
        {
            if (value == null)
            {
                return null;
            }

            Func<AstroObject> obj;
            if (_astroLookup.TryGetValue(value, out obj))
            {
                var owBody = obj?.Invoke();
                if (owBody != null)
                {
                    astros[value] = owBody;
                }
                return owBody;
            }
            return null;
        }

        public static AstroObject getAstro(HeavenlyBody body)
        {
            if (body == null)
            {
                return null;
            }

            AstroObject obj;
            if (!astros.TryGetValue(body, out obj)
                || obj == null || obj?.gameObject == null || !obj.gameObject.activeSelf)
            {
                obj = lookupAstro(body);
            }
            return obj == null || obj?.gameObject == null || !obj.gameObject.activeSelf ? null : obj;
        }

        public static HeavenlyBody[] getAstros()
        {
            var keys = new HeavenlyBody[_astroLookup.Count];
            _astroLookup.Keys.CopyTo(keys, 0);
            return keys;
        }

        private static OWRigidbody lookupBody(HeavenlyBody value)
        {
            if (value == null)
            {
                return null;
            }

            Func<OWRigidbody> obj;
            if (_bodyLookup.TryGetValue(value, out obj))
            {
                var owBody = obj?.Invoke();
                if (owBody != null)
                {
                    bodies[value] = owBody;
                }
                return owBody;
            }
            return null;
        }

        public static OWRigidbody getBody(HeavenlyBody body)
        {
            if (body == null)
            {
                return null;
            }

            OWRigidbody obj;
            if (!bodies.TryGetValue(body, out obj)
                || obj == null || obj?.GetRigidbody() == null || obj?.gameObject == null || !obj.gameObject.activeSelf)
            {
                obj = lookupBody(body);
            }
            return obj == null || obj?.GetRigidbody() == null || obj?.gameObject == null || !obj.gameObject.activeSelf ? null : obj;
        }

        public static HeavenlyBody[] getBodies()
        {
            var keys = new HeavenlyBody[_bodyLookup.Count];
            _bodyLookup.Keys.CopyTo(keys, 0);
            return keys;
        }

        public static List<Tuple<HeavenlyBody, float>> getClosest(Vector3 position)
        {
            return getClosest(position, getBodies(), new HeavenlyBody[0]);
        }

        public static List<Tuple<HeavenlyBody, float>> getClosest(Vector3 position, params HeavenlyBody[] include)
        {
            return getClosest(position, include, new HeavenlyBody[0]);
        }

        private static List<Tuple<HeavenlyBody, float>> getClosest(Vector3 position, HeavenlyBody[] include, HeavenlyBody[] exclude)
        {
            var excl = new HashSet<HeavenlyBody>(exclude);
            return getClosest(position, include, (body) => excl.Contains(body));
        }

        public static List<Tuple<HeavenlyBody, float>> getClosetInfluence(Vector3 position, HeavenlyBody[] include, HeavenlyBody[] exclude)
        {
            var excl = new HashSet<HeavenlyBody>(exclude);
            return Position.getClosest(position, (body) =>
            {
                if (excl.Contains(body))
                {
                    return true;
                }

                var parentState = PositionState.fromCurrentState(body);
                var size = Planet.getSize(body);
                if (parentState == null || size == null)
                {
                    return true;
                }
                else if ((position - parentState.position).sqrMagnitude < size.influence * size.influence)
                {
                    return false;
                }
                return true;
            }, include);
        }

        public static List<Tuple<HeavenlyBody, float>> getClosest(Vector3 position, Predicate<HeavenlyBody> shouldExclude, params HeavenlyBody[] include)
        {
            return getClosest(position, include, shouldExclude);
        }

        private static List<Tuple<HeavenlyBody, float>> getClosest(Vector3 position, HeavenlyBody[] include, Predicate<HeavenlyBody> shouldExclude)
        {
            var obj = new List<Tuple<HeavenlyBody, float>>(include.Length);
            foreach (HeavenlyBody body in include)
            {
                if (!shouldExclude.Invoke(body))
                {
                    var parentState = AbsoluteState.fromCurrentState(body);
                    var distance = parentState?.InverseTransformPoint(position).sqrMagnitude;
                    if (distance.HasValue)
                    {
                        obj.Add(new Tuple<HeavenlyBody, float>(body, distance.Value));
                    }
                }
            }

            if (obj.Count < 1)
            {
                var distance = position.sqrMagnitude;
                obj.Add(new Tuple<HeavenlyBody, float>(HeavenlyBody.None, distance));
            }
            else
            {
                obj.Sort((v1, v2) => v1.Item2.CompareTo(v2.Item2));
            }
            return obj;
        }

        public static KeplerCoordinates getKepler(HeavenlyBody parent, OWRigidbody target)
        {
            var state = PositionState.fromCurrentState(target);
            if (state == null)
            {
                return null;
            }

            return getKepler(parent, state.position, state.velocity);
        }

        public static KeplerCoordinates getKepler(HeavenlyBody parent, Vector3 worldPosition, Vector3 worldVelocity)
        {
            var state = AbsoluteState.fromCurrentState(parent);
            var gravity = Planet.getGravity(parent);

            return getKepler(state, gravity, worldPosition, worldVelocity);
        }

        public static KeplerCoordinates getKepler(AbsoluteState parentState, Gravity parentGravity, Vector3 worldPosition, Vector3 worldVelocity)
        {
            if (parentState == null || parentGravity == null)
            {
                return null;
            }

            var position = worldPosition - parentState.position;
            var velocity = worldVelocity - parentState.velocity;

            return OrbitHelper.toKeplerCoordinates(parentGravity, Time.timeSinceLevelLoad, position, velocity);
        }
    }
}

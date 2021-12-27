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

        private static Dictionary<HeavenlyBody, Func<AstroObject>> _astroLookup = new Dictionary<HeavenlyBody, Func<AstroObject>>();
        private static Dictionary<HeavenlyBody, Func<OWRigidbody>> _bodyLookup = new Dictionary<HeavenlyBody, Func<OWRigidbody>>();
        private static Dictionary<HeavenlyBody, AstroObject> astros = new Dictionary<HeavenlyBody, AstroObject>();
        private static Dictionary<HeavenlyBody, OWRigidbody> bodies = new Dictionary<HeavenlyBody, OWRigidbody>();

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

        static Position()
        {
            _astroLookup.Add(HeavenlyBodies.Sun, () => Locator.GetAstroObject(AstroObject.Name.Sun));
            _astroLookup.Add(HeavenlyBodies.SunStation, () => Locator.GetMinorAstroObject("Sun Station"));
            _astroLookup.Add(HeavenlyBodies.HourglassTwins, () => Locator.GetAstroObject(AstroObject.Name.TowerTwin)?.GetPrimaryBody());
            _astroLookup.Add(HeavenlyBodies.AshTwin, () => Locator.GetAstroObject(AstroObject.Name.TowerTwin));
            _astroLookup.Add(HeavenlyBodies.EmberTwin, () => Locator.GetAstroObject(AstroObject.Name.CaveTwin));
            _astroLookup.Add(HeavenlyBodies.TimberHearth, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth));
            _astroLookup.Add(HeavenlyBodies.TimberHearthProbe, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetSatellite());
            _astroLookup.Add(HeavenlyBodies.Attlerock, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetMoon());
            _astroLookup.Add(HeavenlyBodies.BrittleHollow, () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow));
            _astroLookup.Add(HeavenlyBodies.HollowLantern, () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow)?.GetMoon());
            _astroLookup.Add(HeavenlyBodies.GiantsDeep, () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep));
            _astroLookup.Add(HeavenlyBodies.ProbeCannon, () => Locator.GetAstroObject(AstroObject.Name.ProbeCannon));
            _astroLookup.Add(HeavenlyBodies.DarkBramble, () => Locator.GetAstroObject(AstroObject.Name.DarkBramble));
            _astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Hub, () => Locator.GetMinorAstroObject("Hub Dimension"));
            _astroLookup.Add(HeavenlyBodies.InnerDarkBramble_EscapePod, () => Locator.GetMinorAstroObject("Escape Pod Dimension"));
            _astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Nest, () => Locator.GetMinorAstroObject("Angler Nest Dimension"));
            _astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Feldspar, () => Locator.GetMinorAstroObject("Pioneer Dimension"));
            _astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Gutter, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.ExitOnly == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName()).GetAttachedOWRigidbody().GetComponent<AstroObject>());
            _astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Vessel, () => Locator.GetMinorAstroObject("Vessel Dimension"));
            _astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Maze, () => Locator.GetMinorAstroObject("Cluster Dimension"));
            _astroLookup.Add(HeavenlyBodies.InnerDarkBramble_SmallNest, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.SmallNest == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName()).GetAttachedOWRigidbody().GetComponent<AstroObject>());
            _astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Secret, () => Locator.GetMinorAstroObject("Elsinore Dimension"));
            _astroLookup.Add(HeavenlyBodies.Interloper, () => Locator.GetAstroObject(AstroObject.Name.Comet));
            _astroLookup.Add(HeavenlyBodies.WhiteHole, () => Locator.GetAstroObject(AstroObject.Name.WhiteHole));
            _astroLookup.Add(HeavenlyBodies.WhiteHoleStation, () => Locator.GetAstroObject(AstroObject.Name.WhiteHoleTarget));
            _astroLookup.Add(HeavenlyBodies.Stranger, () => Locator.GetAstroObject(AstroObject.Name.RingWorld));
            _astroLookup.Add(HeavenlyBodies.DreamWorld, () => Locator.GetAstroObject(AstroObject.Name.DreamWorld));
            _astroLookup.Add(HeavenlyBodies.QuantumMoon, () => Locator.GetAstroObject(AstroObject.Name.QuantumMoon));
            _astroLookup.Add(HeavenlyBodies.SatiliteBacker, () => Locator.GetMinorAstroObject("Backer's Satellite"));
            _astroLookup.Add(HeavenlyBodies.SatiliteMapping, () => Helper.getSector(Sector.Name.Unnamed)?.Find(body => "Sector_HearthianMapSatellite".Equals(body?.gameObject?.name))?.GetAttachedOWRigidbody()?.GetComponent<AstroObject>());
            _astroLookup.Add(HeavenlyBodies.EyeOfTheUniverse, () => Helper.getSector(Sector.Name.EyeOfTheUniverse)?.Find(body => true)?.GetAttachedOWRigidbody()?.GetComponent<AstroObject>());
            _astroLookup.Add(HeavenlyBodies.EyeOfTheUniverse_Vessel, () => Helper.getSector(Sector.Name.Vessel)?.Find(body => Sector.Name.EyeOfTheUniverse == body.GetRootSector().GetName())?.GetAttachedOWRigidbody()?.GetComponent<AstroObject>());

            _bodyLookup.Add(HeavenlyBodies.Player, () => Locator.GetPlayerBody());
            _bodyLookup.Add(HeavenlyBodies.Ship, () => Locator.GetShipBody());
            _bodyLookup.Add(HeavenlyBodies.Probe, () => Locator.GetProbe()?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.ModelShip, () => GameObject.Find("ModelRocket_Body")?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.Sun, () => Locator.GetAstroObject(AstroObject.Name.Sun)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.SunStation, () => Locator.GetWarpReceiver(NomaiWarpPlatform.Frequency.SunStation)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.HourglassTwins, () => Locator.GetAstroObject(AstroObject.Name.TowerTwin)?.GetPrimaryBody()?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.AshTwin, () => Locator.GetAstroObject(AstroObject.Name.TowerTwin)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.EmberTwin, () => Locator.GetAstroObject(AstroObject.Name.CaveTwin)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.TimberHearth, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.TimberHearthProbe, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetSatellite()?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.Attlerock, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetMoon()?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.BrittleHollow, () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.HollowLantern, () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow)?.GetMoon()?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.GiantsDeep, () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.ProbeCannon, () => Locator.GetAstroObject(AstroObject.Name.ProbeCannon)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.NomaiProbe, () => Locator.GetAstroObject(AstroObject.Name.ProbeCannon)?.GetComponent<OrbitalProbeLaunchController>()?.GetValue<OWRigidbody>("_probeBody"));
            _bodyLookup.Add(HeavenlyBodies.NomaiEmberTwinShuttle, () => Locator.GetNomaiShuttle(NomaiShuttleController.ShuttleID.HourglassShuttle)?.GetOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.NomaiBrittleHollowShuttle, () => Locator.GetNomaiShuttle(NomaiShuttleController.ShuttleID.BrittleHollowShuttle)?.GetOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.DarkBramble, () => Locator.GetAstroObject(AstroObject.Name.DarkBramble)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Hub, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.Hub == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_EscapePod, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.EscapePod == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Nest, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.AnglerNest == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Feldspar, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.Pioneer == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Gutter, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.ExitOnly == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Vessel, () => Helper.getSector(Sector.Name.VesselDimension)?.Find(body => OuterFogWarpVolume.Name.Vessel == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Maze, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.Cluster == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_SmallNest, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.SmallNest == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Secret, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => body?.GetComponentInChildren<SecretFogWarpVolume>() != null)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.Interloper, () => Locator.GetAstroObject(AstroObject.Name.Comet)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.WhiteHole, () => Locator.GetAstroObject(AstroObject.Name.WhiteHole)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.WhiteHoleStation, () => Locator.GetAstroObject(AstroObject.Name.WhiteHoleTarget)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.Stranger, () => Locator.GetAstroObject(AstroObject.Name.RingWorld)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.DreamWorld, () => Locator.GetAstroObject(AstroObject.Name.DreamWorld)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.QuantumMoon, () => Locator.GetAstroObject(AstroObject.Name.QuantumMoon)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.SatiliteBacker, () => Locator.GetMinorAstroObject("Backer's Satellite")?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.SatiliteMapping, () => Helper.getSector(Sector.Name.Unnamed)?.Find(body => "Sector_HearthianMapSatellite".Equals(body?.gameObject?.name))?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.EyeOfTheUniverse, () => Helper.getSector(Sector.Name.EyeOfTheUniverse)?.Find(body => true)?.GetAttachedOWRigidbody());
            _bodyLookup.Add(HeavenlyBodies.EyeOfTheUniverse_Vessel, () => Helper.getSector(Sector.Name.Vessel)?.Find(body => Sector.Name.EyeOfTheUniverse == body.GetRootSector().GetName())?.GetAttachedOWRigidbody());
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
            foreach (HeavenlyBody pp in HeavenlyBody.GetValues())
            {
                if (getAstro(pp) == body)
                {
                    return pp;
                }
            }

            return HeavenlyBody.None;
        }

        public static HeavenlyBody getParent(HeavenlyBody body)
        {
            return find(getAstro(body)?.GetPrimaryBody());
        }

        private static AstroObject lookupAstro(HeavenlyBody value)
        {
            Func<AstroObject> obj;
            if (_astroLookup.TryGetValue(value, out obj))
            {
                var owBody = obj.Invoke();
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
            Func<OWRigidbody> obj;
            if (_bodyLookup.TryGetValue(value, out obj))
            {
                var owBody = obj.Invoke();
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
                var size = getSize(body);
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

        public static Gravity getGravity(HeavenlyBody parent)
        {
            var parentBody = getBody(parent);
            if (parentBody == null)
            {
                return null;
            }

            float exponent;
            float mass;
            if (parent == HeavenlyBodies.HourglassTwins)
            {
                var emberTwin = Position.getBody(HeavenlyBodies.EmberTwin);
                var ashTwin = Position.getBody(HeavenlyBodies.AshTwin);
                exponent = ((emberTwin?.GetAttachedGravityVolume()?.GetValue<float>("_falloffExponent") ?? 2f) + (ashTwin?.GetAttachedGravityVolume()?.GetValue<float>("_falloffExponent") ?? 2f)) / 2f;
                mass = ((emberTwin?.GetAttachedGravityVolume()?.GetValue<float>("_gravitationalMass") ?? ((emberTwin?.GetMass() ?? 0f) * 1000f)) + (ashTwin?.GetAttachedGravityVolume()?.GetValue<float>("_gravitationalMass") ?? ((ashTwin?.GetMass() ?? 0f) * 1000f))) / 4f;
            }
            else
            {
                exponent = parentBody?.GetAttachedGravityVolume()?.GetValue<float>("_falloffExponent") ?? 2f;
                mass = parentBody?.GetAttachedGravityVolume()?.GetValue<float>("_gravitationalMass") ?? ((parentBody?.GetMass() ?? 0f) * 1000f);
            }

            return Gravity.of(exponent, mass);
        }

        public static Size getSize(HeavenlyBody parent)
        {
            var parentBody = getBody(parent);
            if (parentBody == null)
            {
                return null;
            }
            var gravity = getGravity(parent);

            float size;
            float influence;
            if (parent == HeavenlyBodies.InnerDarkBramble_Hub
                || parent == HeavenlyBodies.InnerDarkBramble_Nest
                || parent == HeavenlyBodies.InnerDarkBramble_Feldspar
                || parent == HeavenlyBodies.InnerDarkBramble_Gutter
                || parent == HeavenlyBodies.InnerDarkBramble_Vessel
                || parent == HeavenlyBodies.InnerDarkBramble_Maze
                || parent == HeavenlyBodies.InnerDarkBramble_SmallNest
                || parent == HeavenlyBodies.InnerDarkBramble_Secret)
            {
                var outerPortal = BramblePortals.getOuterPortal(parent, 0) ?? BramblePortals.getOuterPortal(parent, -1);
                if (outerPortal != null)
                {
                    size = Mathf.Max(outerPortal.GetWarpRadius(), outerPortal.GetExitRadius());
                    influence = size;
                }
                else
                {
                    size = 0f;
                    influence = (float)Math.Sqrt(gravity.mu);
                }
            }
            else if (parent == HeavenlyBodies.HourglassTwins)
            {
                size = 0f;
                if (gravity.exponent < 1.5f)
                {
                    influence = (float)Math.Sqrt(gravity.mu * 400f * 1.5f);
                }
                else
                {
                    influence = (float)Math.Sqrt(gravity.mu);
                }
            }
            else if (parent == HeavenlyBodies.Player)
            {
                size = 1f;
                influence = 1f;
            }
            else if (parent == HeavenlyBodies.Ship
                || parent == HeavenlyBodies.NomaiEmberTwinShuttle
                || parent == HeavenlyBodies.NomaiBrittleHollowShuttle)
            {
                size = 1f;
                influence = 1f;
            }
            else if (parent == HeavenlyBodies.Player)
            {
                size = 1f;
                influence = 1f;
            }
            else if (parent == HeavenlyBodies.Probe
                || parent == HeavenlyBodies.ModelShip
                || parent == HeavenlyBodies.TimberHearthProbe)
            {
                size = 0.5f;
                influence = 0.5f;
            }
            else if (parent == HeavenlyBodies.SunStation
                || parent == HeavenlyBodies.ProbeCannon)
            {
                size = 200f;
                influence = 550f;
            }
            else if (parent == HeavenlyBodies.NomaiProbe)
            {
                size = 35f;
                influence = 100f;
            }
            else if (parent == HeavenlyBodies.WhiteHole)
            {
                size = 30f;
                influence = 200f;
            }
            else if (parent == HeavenlyBodies.WhiteHoleStation)
            {
                size = 30f;
                influence = 100f;
            }
            else if (parent == HeavenlyBodies.Stranger)
            {
                size = 600f;
                influence = 1000f;
            }
            else if (parent == HeavenlyBodies.DreamWorld)
            {
                size = 1000f;
                influence = 1000f;
            }
            else if (parent == HeavenlyBodies.SatiliteBacker
                || parent == HeavenlyBodies.SatiliteMapping)
            {
                size = 5f;
                influence = 100f;
            }
            else if (parent == HeavenlyBodies.EyeOfTheUniverse_Vessel)
            {
                size = 250f;
                influence = 250f;
            }
            else if (parent == HeavenlyBodies.Sun)
            {
                size = 2000f;
                influence = 45000f;
            }
            else
            {
                size = Mathf.Max(parentBody?.GetAttachedGravityVolume()?.GetValue<float>("_upperSurfaceRadius") ?? 0f,
                                parentBody?.GetAttachedGravityVolume()?.GetValue<float>("_lowerSurfaceRadius") ?? 0f,
                                parentBody?.GetAttachedGravityVolume()?.GetValue<float>("_cutoffRadius") ?? 0f);

                if (size == 0)
                {
                    size = parentBody?.GetAttachedGravityVolume()?.GetValue<float>("_alignmentRadius") ?? 0f;
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

            return new Size(size, influence);
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
            var gravity = getGravity(parent);

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

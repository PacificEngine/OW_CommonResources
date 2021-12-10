using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game.Display;
using PacificEngine.OW_CommonResources.Game.State;
using PacificEngine.OW_CommonResources.Geometry;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PacificEngine.OW_CommonResources.Geometry.Orbit;

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

        private delegate AstroObject AstroLookup();
        private delegate OWRigidbody BodyLookup();
        public delegate Vector3 vector();

        public enum HeavenlyBodies
        {
            None,
            Player,
            Ship,
            Probe,
            ModelShip,
            Sun,
            SunStation,
            HourglassTwins,
            AshTwin,
            EmberTwin,
            TimberHearth,
            TimberHearthProbe,
            Attlerock,
            BrittleHollow,
            HollowLantern,
            GiantsDeep,
            ProbeCannon,
            NomaiProbe,
            NomaiEmberTwinShuttle,
            NomaiBrittleHollowShuttle,
            DarkBramble,
            InnerDarkBramble_Hub,
            InnerDarkBramble_EscapePod,
            InnerDarkBramble_Nest,
            InnerDarkBramble_Feldspar,
            InnerDarkBramble_Gutter,
            InnerDarkBramble_Vessel,
            InnerDarkBramble_Maze,
            InnerDarkBramble_Felspar,
            InnerDarkBramble_SmallNest,
            InnerDarkBramble_Secret,
            Interloper,
            WhiteHole,
            WhiteHoleStation,
            Stranger,
            DreamWorld,
            QuantumMoon,
            BackerSatilite,
            MapSatilite,
            EyeOfTheUniverse,
            EyeOfTheUniverse_Vessel
        }

        private static Dictionary<HeavenlyBodies, AstroLookup> astroLookup = new Dictionary<HeavenlyBodies, AstroLookup>();
        private static Dictionary<HeavenlyBodies, BodyLookup> bodyLookup = new Dictionary<HeavenlyBodies, BodyLookup>();
        private static Dictionary<HeavenlyBodies, AstroObject> astros = new Dictionary<HeavenlyBodies, AstroObject>();
        private static Dictionary<HeavenlyBodies, OWRigidbody> bodies = new Dictionary<HeavenlyBodies, OWRigidbody>();

        public static void Start()
        {
            astros.Clear();
            astroLookup.Clear();
            astroLookup.Add(HeavenlyBodies.Sun, () => Locator.GetAstroObject(AstroObject.Name.Sun));
            astroLookup.Add(HeavenlyBodies.SunStation, () => Locator.GetMinorAstroObject("Sun Station"));
            astroLookup.Add(HeavenlyBodies.HourglassTwins, () => Locator.GetAstroObject(AstroObject.Name.TowerTwin)?.GetPrimaryBody());
            astroLookup.Add(HeavenlyBodies.AshTwin, () => Locator.GetAstroObject(AstroObject.Name.TowerTwin));
            astroLookup.Add(HeavenlyBodies.EmberTwin, () => Locator.GetAstroObject(AstroObject.Name.CaveTwin));
            astroLookup.Add(HeavenlyBodies.TimberHearth, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth));
            astroLookup.Add(HeavenlyBodies.TimberHearthProbe, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetSatellite());
            astroLookup.Add(HeavenlyBodies.Attlerock, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetMoon());
            astroLookup.Add(HeavenlyBodies.BrittleHollow, () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow));
            astroLookup.Add(HeavenlyBodies.HollowLantern, () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow)?.GetMoon());
            astroLookup.Add(HeavenlyBodies.GiantsDeep, () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep));
            astroLookup.Add(HeavenlyBodies.ProbeCannon, () => Locator.GetAstroObject(AstroObject.Name.ProbeCannon));
            astroLookup.Add(HeavenlyBodies.DarkBramble, () => Locator.GetAstroObject(AstroObject.Name.DarkBramble));
            astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Hub, () => Locator.GetMinorAstroObject("Hub Dimension"));
            astroLookup.Add(HeavenlyBodies.InnerDarkBramble_EscapePod, () => Locator.GetMinorAstroObject("Escape Pod Dimension"));
            astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Nest, () => Locator.GetMinorAstroObject("Angler Nest Dimension"));
            astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Feldspar, () => Locator.GetMinorAstroObject("Pioneer Dimension"));
            astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Gutter, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.ExitOnly == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName()).GetAttachedOWRigidbody().GetComponent<AstroObject>());
            astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Vessel, () => Locator.GetMinorAstroObject("Vessel Dimension"));
            astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Maze, () => Locator.GetMinorAstroObject("Cluster Dimension"));
            astroLookup.Add(HeavenlyBodies.InnerDarkBramble_SmallNest, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.SmallNest == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName()).GetAttachedOWRigidbody().GetComponent<AstroObject>());
            astroLookup.Add(HeavenlyBodies.InnerDarkBramble_Secret, () => Locator.GetMinorAstroObject("Elsinore Dimension"));
            astroLookup.Add(HeavenlyBodies.Interloper, () => Locator.GetAstroObject(AstroObject.Name.Comet));
            astroLookup.Add(HeavenlyBodies.WhiteHole, () => Locator.GetAstroObject(AstroObject.Name.WhiteHole));
            astroLookup.Add(HeavenlyBodies.WhiteHoleStation, () => Locator.GetAstroObject(AstroObject.Name.WhiteHoleTarget));
            astroLookup.Add(HeavenlyBodies.Stranger, () => Locator.GetAstroObject(AstroObject.Name.RingWorld));
            astroLookup.Add(HeavenlyBodies.DreamWorld, () => Locator.GetAstroObject(AstroObject.Name.DreamWorld));
            astroLookup.Add(HeavenlyBodies.QuantumMoon, () => Locator.GetAstroObject(AstroObject.Name.QuantumMoon));
            astroLookup.Add(HeavenlyBodies.BackerSatilite, () => Locator.GetMinorAstroObject("Backer's Satellite"));
            astroLookup.Add(HeavenlyBodies.MapSatilite, () => Helper.getSector(Sector.Name.Unnamed)?.Find(body => "Sector_HearthianMapSatellite".Equals(body?.gameObject?.name))?.GetAttachedOWRigidbody()?.GetComponent<AstroObject>());
            astroLookup.Add(HeavenlyBodies.EyeOfTheUniverse, () => Helper.getSector(Sector.Name.EyeOfTheUniverse)?.Find(body => true)?.GetAttachedOWRigidbody()?.GetComponent<AstroObject>());
            astroLookup.Add(HeavenlyBodies.EyeOfTheUniverse_Vessel, () => Helper.getSector(Sector.Name.Vessel)?.Find(body => Sector.Name.EyeOfTheUniverse == body.GetRootSector().GetName())?.GetAttachedOWRigidbody()?.GetComponent<AstroObject>());

            bodies.Clear();
            bodyLookup.Clear();
            bodyLookup.Add(HeavenlyBodies.Player, () => Locator.GetPlayerBody());
            bodyLookup.Add(HeavenlyBodies.Ship, () => Locator.GetShipBody());
            bodyLookup.Add(HeavenlyBodies.Probe, () => Locator.GetProbe()?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.ModelShip, () => GameObject.Find("ModelRocket_Body")?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.Sun, () => Locator.GetAstroObject(AstroObject.Name.Sun)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.SunStation, () => Locator.GetWarpReceiver(NomaiWarpPlatform.Frequency.SunStation)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.HourglassTwins, () => Locator.GetAstroObject(AstroObject.Name.TowerTwin)?.GetPrimaryBody()?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.AshTwin, () => Locator.GetAstroObject(AstroObject.Name.TowerTwin)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.EmberTwin, () => Locator.GetAstroObject(AstroObject.Name.CaveTwin)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.TimberHearth, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.TimberHearthProbe, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetSatellite()?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.Attlerock, () => Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetMoon()?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.BrittleHollow, () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.HollowLantern, () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow)?.GetMoon()?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.GiantsDeep, () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.ProbeCannon, () => Locator.GetAstroObject(AstroObject.Name.ProbeCannon)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.NomaiProbe, () => Locator.GetAstroObject(AstroObject.Name.ProbeCannon)?.GetComponent<OrbitalProbeLaunchController>()?.GetValue<OWRigidbody>("_probeBody"));
            bodyLookup.Add(HeavenlyBodies.NomaiEmberTwinShuttle, () => Locator.GetNomaiShuttle(NomaiShuttleController.ShuttleID.HourglassShuttle)?.GetOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.NomaiBrittleHollowShuttle, () => Locator.GetNomaiShuttle(NomaiShuttleController.ShuttleID.BrittleHollowShuttle)?.GetOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.DarkBramble, () => Locator.GetAstroObject(AstroObject.Name.DarkBramble)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Hub, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.Hub == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_EscapePod, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.EscapePod == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Nest, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.AnglerNest == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Feldspar, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.Pioneer == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Gutter, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.ExitOnly == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Vessel, () => Helper.getSector(Sector.Name.VesselDimension)?.Find(body => OuterFogWarpVolume.Name.Vessel == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Maze, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.Cluster == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_SmallNest, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.SmallNest == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Secret, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => body?.GetComponentInChildren<SecretFogWarpVolume>() != null)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.Interloper, () => Locator.GetAstroObject(AstroObject.Name.Comet)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.WhiteHole, () => Locator.GetAstroObject(AstroObject.Name.WhiteHole)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.WhiteHoleStation, () => Locator.GetAstroObject(AstroObject.Name.WhiteHoleTarget)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.Stranger, () => Locator.GetAstroObject(AstroObject.Name.RingWorld)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.DreamWorld, () => Locator.GetAstroObject(AstroObject.Name.DreamWorld)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.QuantumMoon, () => Locator.GetAstroObject(AstroObject.Name.QuantumMoon)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.BackerSatilite, () => Locator.GetMinorAstroObject("Backer's Satellite")?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.MapSatilite, () => Helper.getSector(Sector.Name.Unnamed)?.Find(body => "Sector_HearthianMapSatellite".Equals(body?.gameObject?.name))?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.EyeOfTheUniverse, () => Helper.getSector(Sector.Name.EyeOfTheUniverse)?.Find(body => true)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.EyeOfTheUniverse_Vessel, () => Helper.getSector(Sector.Name.Vessel)?.Find(body => Sector.Name.EyeOfTheUniverse == body.GetRootSector().GetName())?.GetAttachedOWRigidbody());
        }

        public static void Awake()
        {
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

        private static void listValue(string id, string name, float index, OWRigidbody comparison)
        {
            if (comparison)
            {
                var relaitveState = RelativeState.fromClosetInfluence(comparison, Position.HeavenlyBodies.Sun,
                    Position.HeavenlyBodies.Player,
                    Position.HeavenlyBodies.Probe,
                    Position.HeavenlyBodies.Ship,
                    Position.HeavenlyBodies.ModelShip,
                    Position.HeavenlyBodies.NomaiProbe,
                    Position.HeavenlyBodies.NomaiBrittleHollowShuttle,
                    Position.HeavenlyBodies.NomaiEmberTwinShuttle,
                    Position.HeavenlyBodies.TimberHearthProbe);
                listValue(id, name, index, relaitveState);
            }
            else
            {
                listValue(id, name, index, null, Vector3.zero, Vector3.zero);
            }
        }

        private static void listValue(string id, string name, float index, RelativeState relativeState)
        {
            if (relativeState?.surface?.position?.position == null || relativeState?.surface?.position?.velocity == null)
            {
                listValue(id, name, index, null, Vector3.zero, Vector3.zero);
            }
            else
            {
                listValue(id, name, index, relativeState.parent, relativeState.surface.position.position, relativeState.surface.position.velocity);
            }
        }

        private static void listValue(string id, string name, float index, Position.HeavenlyBodies? body, Vector3 position, Vector3 velocity)
        {

            var console = DisplayConsole.getConsole(ConsoleLocation.BottomRight);
            if (body.HasValue)
            {
                debugIds.Add(classId + "." + id + ".Parent");
                debugIds.Add(classId + "." + id + ".Position");
                debugIds.Add(classId + "." + id + ".Velocity");
                console.setElement(classId + "." + id + ".Parent", $"{name} Parent: {body.Value}", index + 0.01f);
                console.setElement(classId + "." + id + ".Position", $"{name} Position: {DisplayConsole.logVector(position)}", index + 0.02f);
                console.setElement(classId + "." + id + ".Velocity", $"{name} Velocity: {DisplayConsole.logVector(velocity)}", index + 0.03f);
            }
        }

        public static HeavenlyBodies find(OWRigidbody body)
        {
            foreach (HeavenlyBodies pp in Enum.GetValues(typeof(HeavenlyBodies)))
            {
                if (getBody(pp) == body)
                {
                    return pp;
                }
            }

            return HeavenlyBodies.None;
        }

        public static HeavenlyBodies find(AstroObject body)
        {
            foreach (HeavenlyBodies pp in Enum.GetValues(typeof(HeavenlyBodies)))
            {
                if (getAstro(pp) == body)
                {
                    return pp;
                }
            }

            return HeavenlyBodies.None;
        }

        public static HeavenlyBodies getParent(HeavenlyBodies body)
        {
            return find(getAstro(body)?.GetPrimaryBody());
        }

        public static HeavenlyBodies getRoot(HeavenlyBodies body)
        {
            switch (body)
            {
                case HeavenlyBodies.Player:
                case HeavenlyBodies.Ship:
                case HeavenlyBodies.Probe:
                case HeavenlyBodies.ModelShip:
                case HeavenlyBodies.Sun:
                case HeavenlyBodies.EyeOfTheUniverse:
                    return HeavenlyBodies.None;
                case HeavenlyBodies.EyeOfTheUniverse_Vessel:
                    return HeavenlyBodies.EyeOfTheUniverse;
                case HeavenlyBodies.SunStation:
                case HeavenlyBodies.HourglassTwins:
                case HeavenlyBodies.AshTwin:
                case HeavenlyBodies.EmberTwin:
                case HeavenlyBodies.TimberHearth:
                case HeavenlyBodies.TimberHearthProbe:
                case HeavenlyBodies.Attlerock:
                case HeavenlyBodies.BrittleHollow:
                case HeavenlyBodies.HollowLantern:
                case HeavenlyBodies.GiantsDeep:
                case HeavenlyBodies.ProbeCannon:
                case HeavenlyBodies.NomaiProbe:
                case HeavenlyBodies.NomaiEmberTwinShuttle:
                case HeavenlyBodies.NomaiBrittleHollowShuttle:
                case HeavenlyBodies.DarkBramble:
                case HeavenlyBodies.InnerDarkBramble_Hub:
                case HeavenlyBodies.InnerDarkBramble_EscapePod:
                case HeavenlyBodies.InnerDarkBramble_Nest:
                case HeavenlyBodies.InnerDarkBramble_Feldspar:
                case HeavenlyBodies.InnerDarkBramble_Gutter:
                case HeavenlyBodies.InnerDarkBramble_Vessel:
                case HeavenlyBodies.InnerDarkBramble_Maze:
                case HeavenlyBodies.InnerDarkBramble_Felspar:
                case HeavenlyBodies.InnerDarkBramble_SmallNest:
                case HeavenlyBodies.InnerDarkBramble_Secret:
                case HeavenlyBodies.Interloper:
                case HeavenlyBodies.WhiteHole:
                case HeavenlyBodies.WhiteHoleStation:
                case HeavenlyBodies.Stranger:
                case HeavenlyBodies.DreamWorld:
                case HeavenlyBodies.QuantumMoon:
                case HeavenlyBodies.MapSatilite:
                    return HeavenlyBodies.Sun;
                default:
                    Helper.helper.Console.WriteLine($"HeavenlyBodies `{body}` is not programmed for getRoot.", MessageType.Warning);
                    return HeavenlyBodies.Sun;
            }
        }

        private static AstroObject lookupAstro(HeavenlyBodies value)
        {
            AstroLookup obj;
            if (astroLookup.TryGetValue(value, out obj))
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

        public static AstroObject getAstro(HeavenlyBodies body)
        {
            AstroObject obj;
            if (!astros.TryGetValue(body, out obj)
                || obj == null || obj?.gameObject == null)
            {
                obj = lookupAstro(body);
            }
            return obj == null || obj?.gameObject == null ? null : obj;
        }

        private static OWRigidbody lookupBody(HeavenlyBodies value)
        {
            BodyLookup obj;
            if (bodyLookup.TryGetValue(value, out obj))
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

        public static OWRigidbody getBody(HeavenlyBodies body)
        {
            OWRigidbody obj;
            if (!bodies.TryGetValue(body, out obj)
                || obj == null || obj?.gameObject == null)
            {
                obj = lookupBody(body);
            }
            return obj == null || obj?.gameObject == null ? null : obj;
        }

        public static List<Tuple<HeavenlyBodies, float>> getClosest(Vector3 position)
        {
            var keys = new HeavenlyBodies[bodyLookup.Count];
            bodyLookup.Keys.CopyTo(keys, 0);
            return getClosest(position, keys, new HeavenlyBodies[0]);
        }

        public static List<Tuple<HeavenlyBodies, float>> getClosest(Vector3 position, params HeavenlyBodies[] include)
        {
            return getClosest(position, include, new HeavenlyBodies[0]);
        }


        public static List<Tuple<HeavenlyBodies, float>> getClosest(Vector3 position, bool isInclude = true, params HeavenlyBodies[] values)
        {
            if (isInclude)
            {
                return getClosest(position, values, new HeavenlyBodies[0]);
            }

            var keys = new HeavenlyBodies[bodyLookup.Count];
            bodyLookup.Keys.CopyTo(keys, 0);
            return getClosest(position, keys, values);
        }

        private static List<Tuple<HeavenlyBodies, float>> getClosest(Vector3 position, HeavenlyBodies[] include, HeavenlyBodies[] exclude)
        {
            var excl = new HashSet<HeavenlyBodies>(exclude);
            return getClosest(position, include, (body) => excl.Contains(body));
        }

        public static List<Tuple<HeavenlyBodies, float>> getClosest(Vector3 position, Predicate<HeavenlyBodies> shouldExclude, params HeavenlyBodies[] include)
        {
            return getClosest(position, include, shouldExclude);
        }

        private static List<Tuple<HeavenlyBodies, float>> getClosest(Vector3 position, HeavenlyBodies[] include, Predicate<HeavenlyBodies> shouldExclude)
        {
            var obj = new List<Tuple<HeavenlyBodies, float>>(include.Length);
            foreach (HeavenlyBodies body in include)
            {
                if (!shouldExclude.Invoke(body))
                {
                    var distance = getBody(body)?.transform?.InverseTransformPoint(position).sqrMagnitude;
                    if (distance.HasValue)
                    {
                        obj.Add(new Tuple<HeavenlyBodies, float>(body, distance.Value));
                    }
                }
            }

            if (obj.Count == 0)
            {
                var distance = (position - (Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero)).sqrMagnitude;
                obj.Add(new Tuple<HeavenlyBodies, float>(HeavenlyBodies.None, distance));
            }
            else
            {
                obj.Sort((v1, v2) => v1.Item2.CompareTo(v2.Item2));
            }
            return obj;
        }

        public static Gravity getGravity(Position.HeavenlyBodies parent)
        {
            var parentBody = getBody(parent);
            if (parentBody == null)
            {
                return null;
            }

            float exponent;
            float mass;
            if (parent == Position.HeavenlyBodies.HourglassTwins)
            {
                var emberTwin = Position.getBody(Position.HeavenlyBodies.EmberTwin);
                var ashTwin = Position.getBody(Position.HeavenlyBodies.AshTwin);
                exponent = ((emberTwin?.GetAttachedGravityVolume()?.GetValue<float>("_falloffExponent") ?? 2f) + (ashTwin?.GetAttachedGravityVolume()?.GetValue<float>("_falloffExponent") ?? 2f)) / 2f;
                mass = ((emberTwin?.GetAttachedGravityVolume()?.GetValue<float>("_gravitationalMass") ?? ((emberTwin?.GetMass() ?? 0f) * 1000f)) + (ashTwin?.GetAttachedGravityVolume()?.GetValue<float>("_gravitationalMass") ?? ((ashTwin?.GetMass() ?? 0f) * 1000f))) / 4f;
            }
            else
            {
                exponent = parentBody?.GetAttachedGravityVolume()?.GetValue<float>("_falloffExponent") ?? 2f;
                mass = parentBody?.GetAttachedGravityVolume()?.GetValue<float>("_gravitationalMass") ?? ((parentBody?.GetMass() ?? 0f) * 1000f);
            }

            return Orbit.Gravity.of(exponent, mass);
        }

        public static Size getSize(Position.HeavenlyBodies parent)
        {
            var parentBody = getBody(parent);
            if (parentBody == null)
            {
                return null;
            }
            var gravity = getGravity(parent);

            float size;
            float influence;
            switch (parent)
            {
                case Position.HeavenlyBodies.InnerDarkBramble_Hub:
                case Position.HeavenlyBodies.InnerDarkBramble_EscapePod:
                case Position.HeavenlyBodies.InnerDarkBramble_Nest:
                case Position.HeavenlyBodies.InnerDarkBramble_Feldspar:
                case Position.HeavenlyBodies.InnerDarkBramble_Gutter:
                case Position.HeavenlyBodies.InnerDarkBramble_Vessel:
                case Position.HeavenlyBodies.InnerDarkBramble_Maze:
                case Position.HeavenlyBodies.InnerDarkBramble_SmallNest:
                case Position.HeavenlyBodies.InnerDarkBramble_Secret:
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
                    break;
                case Position.HeavenlyBodies.HourglassTwins:
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
                    break;
                case Position.HeavenlyBodies.Player:
                    size = 1f;
                    influence = 1f;
                    break;
                case Position.HeavenlyBodies.Ship:
                    size = 15f;
                    influence = 15f;
                    break;
                case Position.HeavenlyBodies.Probe:
                case Position.HeavenlyBodies.ModelShip:
                case Position.HeavenlyBodies.TimberHearthProbe:
                    size = 0.5f;
                    influence = 0.5f;
                    break;
                case Position.HeavenlyBodies.SunStation:
                case Position.HeavenlyBodies.ProbeCannon:
                    size = 200f;
                    influence = 550f;
                    break;
                case Position.HeavenlyBodies.NomaiProbe:
                    size = 35f;
                    influence = 100f;
                    break;
                case Position.HeavenlyBodies.NomaiEmberTwinShuttle:
                case Position.HeavenlyBodies.NomaiBrittleHollowShuttle:
                    size = 15f;
                    influence = 15f;
                    break;
                case Position.HeavenlyBodies.WhiteHole:
                    size = 30f;
                    influence = 200f;
                    break;
                case Position.HeavenlyBodies.WhiteHoleStation:
                    size = 30f;
                    influence = 100f;
                    break;
                case Position.HeavenlyBodies.Stranger:
                    size = 600f;
                    influence = 1000f;
                    break;
                case Position.HeavenlyBodies.DreamWorld:
                    size = 1000f;
                    influence = 1000f;
                    break;
                case Position.HeavenlyBodies.BackerSatilite:
                case Position.HeavenlyBodies.MapSatilite:
                    size = 5f;
                    influence = 100f;
                    break;
                case Position.HeavenlyBodies.EyeOfTheUniverse_Vessel:
                    size = 250f;
                    influence = 250f;
                    break;
                case Position.HeavenlyBodies.Sun:
                case Position.HeavenlyBodies.AshTwin:
                case Position.HeavenlyBodies.EmberTwin:
                case Position.HeavenlyBodies.TimberHearth:
                case Position.HeavenlyBodies.Attlerock:
                case Position.HeavenlyBodies.BrittleHollow:
                case Position.HeavenlyBodies.HollowLantern:
                case Position.HeavenlyBodies.GiantsDeep:
                case Position.HeavenlyBodies.DarkBramble:
                case Position.HeavenlyBodies.Interloper:
                case Position.HeavenlyBodies.QuantumMoon:
                case Position.HeavenlyBodies.EyeOfTheUniverse:
                default:
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
                    }
                    break;

            }

            if (parent == Position.HeavenlyBodies.Sun)
            {
                influence *= 100;
            }

            return new Size(size, influence);
        }
    }
}

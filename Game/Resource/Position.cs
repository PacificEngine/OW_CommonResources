using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game.Display;
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
        private const string classId = "PacificEngine.OW_CommonResources.Game.Resource.Position";

        private static float lastUpdate = 0f;
        private static List<string> debugIds = new List<string>();
        public static bool debugPlayerPosition { get; set; } = false;
        public static bool debugPlanetPosition { get; set; } = false;

        private delegate AstroObject AstroLookup();
        private delegate OWRigidbody BodyLookup();
        public delegate Vector3 vector();

        public enum HeavenlyBodies
        {
            None,
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
            bodyLookup.Add(HeavenlyBodies.DarkBramble, () => Locator.GetAstroObject(AstroObject.Name.DarkBramble)?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Hub, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.Hub ==  body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_EscapePod, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.EscapePod ==  body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Nest, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.AnglerNest == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Feldspar, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.Pioneer == body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
            bodyLookup.Add(HeavenlyBodies.InnerDarkBramble_Gutter, () => Helper.getSector(Sector.Name.BrambleDimension)?.Find(body => OuterFogWarpVolume.Name.ExitOnly ==  body?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())?.GetAttachedOWRigidbody());
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
                    listValue("Player.Root", "Player Root", 10.3f, HeavenlyBodies.Sun, Locator.GetPlayerBody());
                    listValue("Ship.Root", "Ship Root", 10.4f, HeavenlyBodies.Sun, Locator.GetShipBody());
                    listValue("Probe.Root", "Probe Root", 10.5f, HeavenlyBodies.Sun, Locator.GetProbe()?.GetAttachedOWRigidbody());
                }

                if (debugPlanetPosition)
                {
                    var index = 10.6001f;
                    foreach (HeavenlyBodies body in Enum.GetValues(typeof(HeavenlyBodies)))
                    {
                        var astro = getAstro(body);
                        if (astro == null)
                        {
                            continue;
                        }
                        var parent = getParent(body);
                        var kepler = getKepler(body) ?? KelperCoordinates.zero;
                        var gravity = astro?.GetGravityVolume()?.GetStandardGravitationalParameter() ?? 0f;
                        var position = getRelativePosition(parent, astro?.GetAttachedOWRigidbody()) ?? Vector3.zero;
                        var velocity = getRelativeVelocity(parent, astro?.GetAttachedOWRigidbody()) ?? Vector3.zero;
                        var rotation = astro?.GetAttachedOWRigidbody()?.GetRotation() ?? Quaternion.identity;
                        var angularVelocity = astro?.GetAttachedOWRigidbody()?.GetAngularVelocity() ?? Vector3.zero;

                        var id = classId + ".Planet." + body;
                        debugIds.Add(id + ".1");
                        debugIds.Add(id + ".2");
                        debugIds.Add(id + ".3");
                        debugIds.Add(id + ".4");

                        console.setElement(id + ".1", body + " Parent: " + parent + " Gravity: " + gravity, index + 0.0001f);
                        if (kepler != null && kepler != KelperCoordinates.zero)
                        {
                            console.setElement(id + ".2", " Kepler: " + kepler, index + 0.0002f);
                        }
                        console.setElement(id + ".3", " Position: " + position + " Velocity: " + velocity, index + 0.0003f);
                        if (rotation != Quaternion.identity || angularVelocity != Vector3.zero)
                        {
                            console.setElement(id + ".4", " Rotation: " + rotation + " AngularVelocity: " + angularVelocity, index + 0.0004f);
                        }
                        index += 0.0004f;
                    }
                }
            }
        }

        private static void listValue(string id, string name, float index, OWRigidbody comparison)
        {
            if (comparison)
            {
                var list = Position.getClosest(comparison.GetPosition());
                var item = list[0].Item1 == Position.HeavenlyBodies.TimberHearthProbe ? list[1] : list[0];
                listValue(id, name, index, item.Item1, comparison);
            }
            else
            {
                listValue(id, name, index, null, Vector3.zero, Vector3.zero);
            }
        }

        private static void listValue(string id, string name, float index, Position.HeavenlyBodies? body, OWRigidbody comparison)
        {
            if (body.HasValue && comparison)
            {
                var parent = Position.getBody(body.Value);
                if (parent)
                {
                    var p = parent.transform.InverseTransformPoint(comparison.GetPosition());
                    var v = comparison.GetVelocity() - (parent.GetPointTangentialVelocity(comparison.GetPosition()) + parent.GetVelocity());
                    listValue(id, name, index, body, p, v);
                }
                else
                {
                    listValue(id, name, index, null, Vector3.zero, Vector3.zero);
                }
            }
            else
            {
                listValue(id, name, index, null, Vector3.zero, Vector3.zero);
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
                console.setElement(classId + "." + id + ".Parent", name + " Parent: " + body.Value, index + 0.01f);
                console.setElement(classId + "." + id + ".Position", name + " Position: " + position, index + 0.02f);
                console.setElement(classId + "." + id + ".Velocity", name + " Velocity: " + velocity, index + 0.03f);
            }
        }

        public static HeavenlyBodies getParent(HeavenlyBodies body)
        {
            var parent = getAstro(body)?.GetPrimaryBody();
            foreach (HeavenlyBodies pp in Enum.GetValues(typeof(HeavenlyBodies)))
            {
                if (getAstro(pp) == parent)
                {
                    return pp;
                }
            }

            return HeavenlyBodies.None;
        }

        public static HeavenlyBodies getRoot(HeavenlyBodies body)
        {
            switch(body)
            {
                case HeavenlyBodies.Sun:
                case HeavenlyBodies.EyeOfTheUniverse:
                    return HeavenlyBodies.None;
                case HeavenlyBodies.EyeOfTheUniverse_Vessel:
                    return HeavenlyBodies.EyeOfTheUniverse;
                case HeavenlyBodies.AshTwin:
                case HeavenlyBodies.EmberTwin:
                    return HeavenlyBodies.HourglassTwins;
                case HeavenlyBodies.SunStation:
                case HeavenlyBodies.HourglassTwins:
                case HeavenlyBodies.TimberHearth:
                case HeavenlyBodies.TimberHearthProbe:
                case HeavenlyBodies.Attlerock:
                case HeavenlyBodies.BrittleHollow:
                case HeavenlyBodies.HollowLantern:
                case HeavenlyBodies.GiantsDeep:
                case HeavenlyBodies.ProbeCannon:
                case HeavenlyBodies.NomaiProbe:
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
                    Helper.helper.Console.WriteLine("HeavenlyBodies `" + body + "` is not programmed for getRoot.", MessageType.Warning);
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
            return getClosest(position, keys);
        }

        public static List<Tuple<HeavenlyBodies, float>> getClosest(Vector3 position, params HeavenlyBodies[] include)
        {
            var obj = new List<Tuple<HeavenlyBodies, float>>(include.Length);
            foreach (HeavenlyBodies body in include)
            {
                var distance = getBody(body)?.transform?.InverseTransformPoint(position).sqrMagnitude;
                obj.Add(new Tuple<HeavenlyBodies, float>(body, distance.HasValue ? distance.Value : float.PositiveInfinity));
            }
            obj.Sort((v1, v2) => v1.Item2.CompareTo(v2.Item2));
            return obj;
        }

        public static Vector3? getRelativePosition(HeavenlyBodies parent, OWRigidbody target)
        {
            var parentPosition = getBody(parent)?.GetPosition();
            if (!parentPosition.HasValue)
            {
                return null;
            }
            return target.GetPosition() - parentPosition.Value;
        }

        public static Vector3? getRelativeVelocity(HeavenlyBodies parent, OWRigidbody target)
        {
            var parentVelocity = getBody(parent)?.GetVelocity();
            if (!parentVelocity.HasValue)
            {
                return null;
            }
            return target.GetVelocity() - parentVelocity.Value;
        }

        public static Vector3? getSurfaceVelocity(HeavenlyBodies parent, OWRigidbody target)
        {
            var parentVelocity = getBody(parent)?.GetPointTangentialVelocity(target.GetPosition());
            if (!parentVelocity.HasValue)
            {
                return null;
            }
            return target.GetVelocity() - parentVelocity.Value;
        }

        public static KelperCoordinates getKepler(HeavenlyBodies body)
        {
            var astro = getAstro(body);
            if (astro == null)
            {
                return null;
            }
            var parent = getParent(body);
            if (parent == HeavenlyBodies.None)
            {
                return null;
            }
            var parentAstro = getAstro(parent);

            var gravity = parentAstro?.GetGravityVolume()?.GetStandardGravitationalParameter();
            if (!gravity.HasValue)
            {
                return null;
            }

            var position = getRelativePosition(parent, astro?.GetAttachedOWRigidbody()) ?? Vector3.zero;
            var velocity = getRelativeVelocity(parent, astro?.GetAttachedOWRigidbody()) ?? Vector3.zero;
            return Orbit.toKeplerCoordinates(GravityVolume.GRAVITATIONAL_CONSTANT, gravity.Value, 0f, position, velocity);
        }
    }
}

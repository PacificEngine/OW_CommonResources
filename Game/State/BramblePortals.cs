
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PacificEngine.OW_CommonResources.Game.Resource;
using UnityEngine;
using PacificEngine.OW_CommonResources.Game.Display;

namespace PacificEngine.OW_CommonResources.Game.State
{
    public static class BramblePortals
    {
        private const string classId = "PacificEngine.OW_CommonResources.Game.State.BramblePortals";
        public static bool debugMode { get; set; } = false;
        private static float _lastUpdate = 0f;
        private static List<string> debugIds = new List<string>();

        public delegate void BrambleWarpEvent(FogWarpDetector.Name warpObject, bool isInnerPortal, Tuple<Position.HeavenlyBodies, int> sender, Tuple<Position.HeavenlyBodies, int> reciever);

        private static int processingFrame = -1;
        private static HashSet<OuterFogWarpVolume> alreadyProcessed = new HashSet<OuterFogWarpVolume>();
        private static List<FogWarpVolume> unprocessedPortals = new List<FogWarpVolume>();
        private static Dictionary<Position.HeavenlyBodies, List<Tuple<FogWarpVolume, float>>> _portals = new Dictionary<Position.HeavenlyBodies, List<Tuple<FogWarpVolume, float>>>();
        private static Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>> _outerPortalMap = defaultMapping.Item1;
        private static Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>> _innerPortalMap = defaultMapping.Item2;
        private static Boolean requireUpdate = false;

        public static List<Position.HeavenlyBodies> bodies
        {
            get
            {
                updateLists();
                return new List<Position.HeavenlyBodies>(_portals.Keys);
            }
        }

        public static List<Tuple<Position.HeavenlyBodies, FogWarpVolume>> portals
        {
            get
            {
                updateLists();
                var obj = new List<Tuple<Position.HeavenlyBodies, FogWarpVolume>>();
                foreach (var key in _portals.Keys)
                {
                    obj.AddRange(_portals[key].ConvertAll(x => Tuple.Create(key, x.Item1)));
                }
                return obj;
            }
        }

        public static List<Tuple<Position.HeavenlyBodies, SecretFogWarpVolume>> secretPortals
        {
            get
            {
                return portals.FindAll(x => x.Item2 is SecretFogWarpVolume).ConvertAll(x => Tuple.Create(x.Item1, x.Item2 as SecretFogWarpVolume));
            }
        }

        public static List<Tuple<Position.HeavenlyBodies, OuterFogWarpVolume>> outerPortals
        {
            get
            {
                return portals.FindAll(x => (x.Item2 is OuterFogWarpVolume) && !(x.Item2 is SecretFogWarpVolume)).ConvertAll(x => Tuple.Create(x.Item1, x.Item2 as OuterFogWarpVolume));
            }
        }

        public static List<Tuple<Position.HeavenlyBodies, InnerFogWarpVolume>> innerPortals
        {
            get
            {
                return portals.FindAll(x => x.Item2 is InnerFogWarpVolume).ConvertAll(x => Tuple.Create(x.Item1, x.Item2 as InnerFogWarpVolume));
            }
        }

        public static List<Tuple<Position.HeavenlyBodies, CapsuleFogWarpVolume>> capsulePortals
        {
            get
            {
                return portals.FindAll(x => x.Item2 is CapsuleFogWarpVolume).ConvertAll(x => Tuple.Create(x.Item1, x.Item2 as CapsuleFogWarpVolume));
            }
        }

        public static Tuple<Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>>, Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>>> mapping
        {
            get
            {
                var allPortals = portals;
                if (allPortals.Count < 1)
                {
                    return Tuple.Create(_outerPortalMap, _innerPortalMap);
                }

                updateLists();
                var outerPortalMap = new Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>>();
                var innerPortalMap = new Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>>();
                foreach (var portal in allPortals)
                {
                    var index = findIndex(portal.Item2, portal.Item1);
                    if (index.HasValue && portal.Item2 is SphericalFogWarpVolume)
                    {
                        if (portal.Item2 is InnerFogWarpVolume)
                        {
                            innerPortalMap[Tuple.Create(portal.Item1, index.Value)] = find((portal.Item2 as SphericalFogWarpVolume).GetLinkedFogWarpVolume());
                        }
                        else
                        {
                            outerPortalMap[Tuple.Create(portal.Item1, index.Value)] = find((portal.Item2 as SphericalFogWarpVolume).GetLinkedFogWarpVolume());
                        }
                    }
                }
                return Tuple.Create(outerPortalMap, innerPortalMap);
            }
            set
            {
                if (_outerPortalMap == null || _innerPortalMap == null)
                {
                    var defaults = defaultMapping;
                    if (_outerPortalMap == null)
                    {
                        _outerPortalMap = new Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>>();
                        foreach (var outerMappping in defaults.Item1)
                        {
                            _outerPortalMap[outerMappping.Key] = outerMappping.Value;
                        }
                    }
                    if (_innerPortalMap == null)
                    {
                        _innerPortalMap = new Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>>();
                        foreach (var innerMapping in defaults.Item2)
                        {
                            _innerPortalMap[innerMapping.Key] = innerMapping.Value;
                        }
                    }
                }

                foreach (var outerMappping in value.Item1)
                {
                    _outerPortalMap[outerMappping.Key] = outerMappping.Value;
                }
                foreach (var innerMapping in value.Item2)
                {
                    _innerPortalMap[innerMapping.Key] = innerMapping.Value;
                }

                requireUpdate = true;
                updateLists();
            }
        }

        public static Tuple<Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>>, Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>>> defaultMapping
        {
            get
            {
                var outerPortalMap = new Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>>();
                var innerPortalMap = new Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>>();

                outerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Hub, 0), Tuple.Create(Position.HeavenlyBodies.DarkBramble, 0));
                outerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_EscapePod, 0), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Hub, 2));
                outerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Nest, 0), Tuple.Create(Position.HeavenlyBodies.DarkBramble, 0));
                outerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Feldspar, 0), Tuple.Create(Position.HeavenlyBodies.DarkBramble, 0));
                outerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, 0), Tuple.Create(Position.HeavenlyBodies.DarkBramble, 0));
                outerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Vessel, 0), Tuple.Create(Position.HeavenlyBodies.DarkBramble, 0));
                outerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Maze, 0), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Hub, 3));
                outerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_SmallNest, 0), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Hub, 1));
                outerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Secret, -1), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, -1));

                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.TimberHearth, -1), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Feldspar, 0));

                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.DarkBramble, 0), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Hub, 0));

                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Hub, 0), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Nest, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Hub, 1), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_SmallNest, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Hub, 2), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_EscapePod, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Hub, 3), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Maze, 0));

                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_EscapePod, 0), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Nest, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_EscapePod, -1), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Vessel, 0));

                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Nest, 0), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Vessel, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Nest, 1), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Nest, 2), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Nest, 3), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, 0));

                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Feldspar, -1), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Nest, 0));

                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, 0), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, -1), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Secret, -1));

                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Maze, -1), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Feldspar, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Maze, 0), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Maze, 1), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Feldspar, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Maze, 2), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Maze, 3), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Maze, 4), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Maze, 5), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Maze, 6), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Feldspar, 0));
                innerPortalMap.Add(Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Maze, 7), Tuple.Create(Position.HeavenlyBodies.InnerDarkBramble_Gutter, 0));

                return Tuple.Create(outerPortalMap, innerPortalMap);
            }
        }

        public static event BrambleWarpEvent onBrambleWarp;

        private static void reset()
        {
            processingFrame = -1;
            alreadyProcessed.Clear();
            unprocessedPortals.Clear();
            requireUpdate = false;

            _portals.Clear();
            _portals[Position.HeavenlyBodies.TimberHearth] = new List<Tuple<FogWarpVolume, float>>();
            _portals[Position.HeavenlyBodies.DarkBramble] = new List<Tuple<FogWarpVolume, float>>();
            _portals[Position.HeavenlyBodies.InnerDarkBramble_Hub] = new List<Tuple<FogWarpVolume, float>>();
            _portals[Position.HeavenlyBodies.InnerDarkBramble_EscapePod] = new List<Tuple<FogWarpVolume, float>>();
            _portals[Position.HeavenlyBodies.InnerDarkBramble_Nest] = new List<Tuple<FogWarpVolume, float>>();
            _portals[Position.HeavenlyBodies.InnerDarkBramble_Feldspar] = new List<Tuple<FogWarpVolume, float>>();
            _portals[Position.HeavenlyBodies.InnerDarkBramble_Gutter] = new List<Tuple<FogWarpVolume, float>>();
            _portals[Position.HeavenlyBodies.InnerDarkBramble_Vessel] = new List<Tuple<FogWarpVolume, float>>();
            _portals[Position.HeavenlyBodies.InnerDarkBramble_Maze] = new List<Tuple<FogWarpVolume, float>>();
            _portals[Position.HeavenlyBodies.InnerDarkBramble_SmallNest] = new List<Tuple<FogWarpVolume, float>>();
            _portals[Position.HeavenlyBodies.InnerDarkBramble_Secret] = new List<Tuple<FogWarpVolume, float>>();
        }

        public static void Start()
        {
            reset();

            Helper.helper.HarmonyHelper.AddPostfix<FogWarpVolume>("Awake", typeof(BramblePortals), "onFogWarpVolumeAwake");
            Helper.helper.HarmonyHelper.AddPrefix<OuterFogWarpVolume>("PropagateCanvasMarkerOutwards", typeof(BramblePortals), "onOuterFogWarpVolumePropagateCanvasMarkerOutwards");
        }

        public static void Awake()
        {
        }

        public static void Destroy()
        {
        }

        public static void Update()
        {
            var console = DisplayConsole.getConsole(ConsoleLocation.BottomLeft);
            if (debugMode && _portals.Count > 0)
            {
                if (Time.time - _lastUpdate > 0.2f)
                {
                    _lastUpdate = Time.time;
                    foreach (var id in debugIds)
                    {
                        console.setElement(id, "", 0f);
                    }

                    var map = mapping;
                    var outer = map.Item1;
                    var inner = map.Item2;
                    float index = 11.1f;

                    var allBodies = bodies;
                    allBodies.Sort();

                    console.setElement(getId("Outer"), "Bramble Outer Portals", 11.09f);
                    foreach (var body in allBodies)
                    {
                        List<Tuple<FogWarpVolume, float>> portals;
                        if (_portals.TryGetValue(body, out portals))
                        {
                            foreach (var portal in portals)
                            {
                                if (portal.Item1 is OuterFogWarpVolume)
                                {
                                    var portalIndex = findIndex(portal.Item1, body);
                                    if (portalIndex.HasValue)
                                    {
                                        var portalValue = Tuple.Create(body, portalIndex.Value);
                                        console.setElement(getId("Outer." + portalValue.Item1 + "." + portalValue.Item2), getString(portalValue, ref outer), index);
                                        index += 0.01f;
                                    }
                                }
                            }
                        }
                    }

                    console.setElement(getId("Inner"), "Bramble Inner Portals", index);
                    index += 0.01f;
                    foreach (var body in allBodies)
                    {
                        List<Tuple<FogWarpVolume, float>> portals;
                        if (_portals.TryGetValue(body, out portals))
                        {
                            foreach (var portal in portals)
                            {
                                if (portal.Item1 is InnerFogWarpVolume)
                                {
                                    var portalIndex = findIndex(portal.Item1, body);
                                    if (portalIndex.HasValue)
                                    {
                                        var portalValue = Tuple.Create(body, portalIndex.Value);
                                        console.setElement(getId("Inner." + portalValue.Item1 + "." + portalValue.Item2), getString(portalValue, ref inner), index);
                                        index += 0.01f;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var id in debugIds)
                {
                    console.setElement(id, "", 0f);
                }
            }

            updateLists();
        }

        public static void FixedUpdate()
        {
        }

        private static string getId(string postfix)
        {
            var id = classId + "." + postfix;
            debugIds.Add(id);
            return id;
        }

        private static string getString(Tuple<Position.HeavenlyBodies, int> sender, ref Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>> map)
        {
            Tuple<Position.HeavenlyBodies, int> reciever;
            if (map.TryGetValue(sender, out reciever))
            {
                if (reciever != null)
                {
                    return $"{sender.Item1}:{sender.Item2} -> {reciever.Item1}:{reciever.Item2}"; ;
                }
                else
                {
                    return $"{sender.Item1}:{sender.Item2} -> Nowhere"; ;
                }
            }
            else
            {
                return $"{sender.Item1}:{sender.Item2} -> Nowhere"; ;
            }
        }

        public static InnerFogWarpVolume getInnerPortal(Position.HeavenlyBodies body, int index)
        {
            List<Tuple<FogWarpVolume, float>> portal;
            if (_portals.TryGetValue(body, out portal))
            {
                if (index < 0)
                {
                    var element = portal.FindAll(x => (x.Item1 is InnerFogWarpVolume) && x.Item1.IsProbeOnly()).ElementAtOrDefault((-1 * index) - 1);
                    return element == null ? null : element.Item1 as InnerFogWarpVolume;
                }
                else
                {
                    var element = portal.FindAll(x => (x.Item1 is InnerFogWarpVolume) && !x.Item1.IsProbeOnly()).ElementAtOrDefault(index);
                    return element == null ? null : element.Item1 as InnerFogWarpVolume;
                }
            }
            return null;
        }

        public static OuterFogWarpVolume getOuterPortal(Position.HeavenlyBodies body, int index)
        {
            List<Tuple<FogWarpVolume, float>> portal;
            if (_portals.TryGetValue(body, out portal))
            {
                if (index < 0)
                {
                    var element = portal.FindAll(x => (x.Item1 is SecretFogWarpVolume)).ElementAtOrDefault((-1 * index) - 1);
                    return element == null ? null : element.Item1 as OuterFogWarpVolume;
                }
                else
                {
                    var element = portal.FindAll(x => (x.Item1 is OuterFogWarpVolume) && !(x.Item1 is SecretFogWarpVolume)).ElementAtOrDefault(index);
                    return element == null ? null : element.Item1 as OuterFogWarpVolume;
                }
            }
            return null;
        }

        public static List<InnerFogWarpVolume> getInnerPortals(OuterFogWarpVolume outer)
        {
            return outer.GetValue<List<InnerFogWarpVolume>>("_senderWarps");
        }

        public static Tuple<Position.HeavenlyBodies, int> find(FogWarpVolume volume)
        {
            Position.HeavenlyBodies? body = findBody(volume);
            if (!body.HasValue)
                return null;

            int? index = findIndex(volume, body.Value);
            if (!index.HasValue)
                return null;
            return Tuple.Create(body.Value, index.Value);
        }

        private static Position.HeavenlyBodies? findBody(FogWarpVolume volume)
        {
            if (volume == null)
            {
                return null;
            }
            if (volume is SecretFogWarpVolume)
            {
                return Position.HeavenlyBodies.InnerDarkBramble_Secret;
            }
            if (volume == null 
                || volume?.GetAttachedOWRigidbody() == null
                || volume?.GetAttachedOWRigidbody()?.GetComponentInChildren<OuterFogWarpVolume>() == null
                || volume?.GetAttachedOWRigidbody()?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName() == null)
            {
                var state = PositionState.fromCurrentState(volume.gameObject);
                if (state == null)
                    return null;
                return getClosest(state.position).Item1;
            }

            switch (volume?.GetAttachedOWRigidbody()?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())
            {
                case OuterFogWarpVolume.Name.Hub:
                    return Position.HeavenlyBodies.InnerDarkBramble_Hub;
                case OuterFogWarpVolume.Name.EscapePod:
                    return Position.HeavenlyBodies.InnerDarkBramble_EscapePod;
                case OuterFogWarpVolume.Name.AnglerNest:
                    return Position.HeavenlyBodies.InnerDarkBramble_Nest;
                case OuterFogWarpVolume.Name.Pioneer:
                    return Position.HeavenlyBodies.InnerDarkBramble_Feldspar;
                case OuterFogWarpVolume.Name.ExitOnly:
                    return Position.HeavenlyBodies.InnerDarkBramble_Gutter;
                case OuterFogWarpVolume.Name.Vessel:
                    return Position.HeavenlyBodies.InnerDarkBramble_Vessel;
                case OuterFogWarpVolume.Name.Cluster:
                    return Position.HeavenlyBodies.InnerDarkBramble_Maze;
                case OuterFogWarpVolume.Name.SmallNest:
                    return Position.HeavenlyBodies.InnerDarkBramble_SmallNest;
                case null:
                default:
                    var state = PositionState.fromCurrentState(volume.gameObject);
                    if (state == null)
                        return null;
                    return getClosest(state.position).Item1;
            }
        }

        private static int? findIndex(FogWarpVolume volume, Position.HeavenlyBodies body)
        {
            if (volume is SecretFogWarpVolume)
            {
                var index = secretPortals.FindAll(x => x.Item1 == body).FindIndex(x => x.Item2 == volume);
                if (index < 0)
                {
                    return null;
                }
                return -1 * index - 1;
            }
            else if (volume is OuterFogWarpVolume)
            {
                var index = outerPortals.FindAll(x => x.Item1 == body).FindIndex(x => x.Item2 == volume);
                if (index < 0)
                {
                    return null;
                }
                return index;
            }
            else if (volume is InnerFogWarpVolume)
            {
                var index = innerPortals.FindAll(x => x.Item1 == body && x.Item2.IsProbeOnly() == volume.IsProbeOnly()).FindIndex(x => x.Item2 == volume);
                if (index < 0)
                {
                    return null;
                }
                if (volume.IsProbeOnly())
                {
                    return -1 * index - 1;
                }
                return index;
            }
            else
            {
                return null;
            }
        }

        public static void remapOuterPortal(Tuple<Position.HeavenlyBodies, int> start, Tuple<Position.HeavenlyBodies, int> end)
        {
            _remapOuterPortal(start, end);
            if (end != null && start != null)
            {
                var map = _outerPortalMap ?? mapping.Item1;
                map[start] = end;
                _outerPortalMap = map;
            }
        }

        public static void remapInnerPortal(Tuple<Position.HeavenlyBodies, int> start, Tuple<Position.HeavenlyBodies, int> end)
        {
            _remapInnerPortal(start, end);
            if (start != null && end != null)
            {
                var map = _innerPortalMap ?? mapping.Item2;
                map[start] = end;
                _innerPortalMap = map;
            }
        }

        private static void _remapOuterPortal(Tuple<Position.HeavenlyBodies, int> start, Tuple<Position.HeavenlyBodies, int> end)
        {
            var outerPortal = start == null ? null : getOuterPortal(start.Item1, start.Item2);
            var innerPortal = end == null ? null : getInnerPortal(end.Item1, end.Item2);
            if (innerPortal != null && outerPortal != null)
            {
                outerPortal.SetValue("_linkedInnerWarpVolume", innerPortal);
            }
        }

        private static void _remapInnerPortal(Tuple<Position.HeavenlyBodies, int> start, Tuple<Position.HeavenlyBodies, int> end)
        {
            var outerPortal = end == null ? null : getOuterPortal(end.Item1, end.Item2);
            var innerPortal = start == null ? null : getInnerPortal(start.Item1, start.Item2);
            if (innerPortal != null && outerPortal != null)
            {
                innerPortal.GetValue<OuterFogWarpVolume>("_linkedOuterWarpVolume").GetValue<List<InnerFogWarpVolume>>("_senderWarps").Remove(innerPortal);
                innerPortal.SetValue("_linkedOuterWarpVolume", outerPortal);
                innerPortal.SetValue("_linkedOuterWarpName", outerPortal.GetName());
                outerPortal.RegisterSenderWarp(innerPortal);
            }
        }

        private static void updateLists()
        {
            if (requireUpdate)
            {
                foreach (FogWarpVolume volume in unprocessedPortals)
                {
                    if (volume != null && volume?.gameObject != null)
                    {
                        var state = PositionState.fromCurrentState(volume.gameObject);
                        Tuple<Position.HeavenlyBodies, float> parent;
                        parent = getClosest(state.position);
                        _portals[parent.Item1].Add(Tuple.Create(volume, parent.Item2));
                        volume.OnWarpDetector += (detector) => onWarp(volume, parent.Item1, detector);
                    }
                }
                unprocessedPortals.Clear();

                foreach (Position.HeavenlyBodies key in _portals.Keys)
                {
                    var volumes = _portals[key];
                    volumes.RemoveAll(v => v?.Item1 == null || v?.Item1?.gameObject == null);
                    volumes.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                }
                requireUpdate = false;

                doMapping();
            }
        }

        private static void doMapping()
        {
            var currentMapping = Tuple.Create(_outerPortalMap, _innerPortalMap);
            foreach (var outerMapping in currentMapping.Item1)
            {
                _remapOuterPortal(outerMapping.Key, outerMapping.Value);
            }

            foreach (var innerMapping in currentMapping.Item2)
            {
                _remapInnerPortal(innerMapping.Key, innerMapping.Value);
            }
        }

        private static Tuple<Position.HeavenlyBodies, float> getClosest(Vector3 position)
        {
            var keys = new Position.HeavenlyBodies[_portals.Count];
            _portals.Keys.CopyTo(keys, 0);
            return Position.getClosest(position, keys)[0];
        }

        private static void onFogWarpVolumeAwake(FogWarpVolume __instance)
        {
            unprocessedPortals.Add(__instance);
            requireUpdate = true;
        }

        private static bool onOuterFogWarpVolumePropagateCanvasMarkerOutwards(ref OuterFogWarpVolume __instance, ref CanvasMarker marker, ref bool addMarker, ref float warpDist)
        {
            if (processingFrame != Time.frameCount)
            {
                processingFrame = Time.frameCount;
                alreadyProcessed.Clear();
                return true;
            }

            if (!alreadyProcessed.Contains(__instance))
            {
                alreadyProcessed.Add(__instance);
                return true;
            }
            
            return false;
        }

        private static void onWarp(FogWarpVolume instance, Position.HeavenlyBodies portalParent, FogWarpDetector warpedObject)
        {
            if (instance is SphericalFogWarpVolume)
            {
                var startIndex = findIndex(instance, portalParent);
                if (startIndex.HasValue)
                {
                    var start = Tuple.Create(portalParent, startIndex.Value);
                    var linked = (instance as SphericalFogWarpVolume).GetLinkedFogWarpVolume();
                    onBrambleWarp?.Invoke(warpedObject.GetValue<FogWarpDetector.Name>("_name"), instance is InnerFogWarpVolume, start, find(linked));
                }
            }
        }
    }
}


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


        private static float _lastUpdate = 0f;
        private static List<string> debugIds = new List<string>();
        public static bool _enabledManagement { get; set; } = false;
        public static bool enabledManagement { get { return _enabledManagement; } set { _enabledManagement = value; requireUpdate = true; } }
        public static int logFrequency { get; set; } = -1;
        public static bool debugMode { get; set; } = false;

        public delegate void BrambleWarpEvent(FogWarpDetector.Name warpObject, bool isInnerPortal, Tuple<HeavenlyBody, int> sender, Tuple<HeavenlyBody, int> reciever);

        private static Boolean requireUpdate = false;
        private static int processingFrame = -1;
        private static HashSet<OuterFogWarpVolume> alreadyProcessed = new HashSet<OuterFogWarpVolume>();
        private static List<FogWarpVolume> unprocessedPortals = new List<FogWarpVolume>();
        private static Dictionary<HeavenlyBody, List<Tuple<FogWarpVolume, float>>> _portals = new Dictionary<HeavenlyBody, List<Tuple<FogWarpVolume, float>>>();
        private static Tuple<Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>, Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>> _defaultMapping = standardMapping;
        private static Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>> _outerPortalMap = standardMapping.Item1;
        private static Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>> _innerPortalMap = standardMapping.Item2;

        public static List<HeavenlyBody> bodies
        {
            get
            {
                updateLists();
                return new List<HeavenlyBody>(_portals.Keys);
            }
        }

        public static List<Tuple<HeavenlyBody, FogWarpVolume>> portals
        {
            get
            {
                updateLists();
                var obj = new List<Tuple<HeavenlyBody, FogWarpVolume>>();
                foreach (var key in _portals.Keys)
                {
                    obj.AddRange(_portals[key].ConvertAll(x => Tuple.Create(key, x.Item1)));
                }
                return obj;
            }
        }

        public static List<Tuple<HeavenlyBody, SecretFogWarpVolume>> secretPortals
        {
            get
            {
                return portals.FindAll(x => x.Item2 is SecretFogWarpVolume).ConvertAll(x => Tuple.Create(x.Item1, x.Item2 as SecretFogWarpVolume));
            }
        }

        public static List<Tuple<HeavenlyBody, OuterFogWarpVolume>> outerPortals
        {
            get
            {
                return portals.FindAll(x => (x.Item2 is OuterFogWarpVolume) && !(x.Item2 is SecretFogWarpVolume)).ConvertAll(x => Tuple.Create(x.Item1, x.Item2 as OuterFogWarpVolume));
            }
        }

        public static List<Tuple<HeavenlyBody, InnerFogWarpVolume>> innerPortals
        {
            get
            {
                return portals.FindAll(x => x.Item2 is InnerFogWarpVolume).ConvertAll(x => Tuple.Create(x.Item1, x.Item2 as InnerFogWarpVolume));
            }
        }

        public static List<Tuple<HeavenlyBody, CapsuleFogWarpVolume>> capsulePortals
        {
            get
            {
                return portals.FindAll(x => x.Item2 is CapsuleFogWarpVolume).ConvertAll(x => Tuple.Create(x.Item1, x.Item2 as CapsuleFogWarpVolume));
            }
        }

        public static Tuple<Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>, Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>> standardMapping
        {
            get
            {
                var outerPortalMap = new Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>();
                var innerPortalMap = new Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>();

                outerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Hub, 0), Tuple.Create(HeavenlyBodies.DarkBramble, 0));
                outerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_EscapePod, 0), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Hub, 2));
                outerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Nest, 0), Tuple.Create(HeavenlyBodies.DarkBramble, 0));
                outerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Feldspar, 0), Tuple.Create(HeavenlyBodies.DarkBramble, 0));
                outerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, 0), Tuple.Create(HeavenlyBodies.DarkBramble, 0));
                outerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Vessel, 0), Tuple.Create(HeavenlyBodies.DarkBramble, 0));
                outerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Maze, 0), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Hub, 3));
                outerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_SmallNest, 0), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Hub, 1));
                outerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Secret, -1), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, -1));

                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.TimberHearth, -1), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Feldspar, 0));

                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.DarkBramble, 0), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Hub, 0));

                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Hub, 0), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Nest, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Hub, 1), Tuple.Create(HeavenlyBodies.InnerDarkBramble_SmallNest, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Hub, 2), Tuple.Create(HeavenlyBodies.InnerDarkBramble_EscapePod, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Hub, 3), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Maze, 0));

                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_EscapePod, 0), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Nest, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_EscapePod, -1), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Vessel, 0));

                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Nest, 0), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Vessel, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Nest, 1), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Nest, 2), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Nest, 3), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, 0));

                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Feldspar, -1), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Nest, 0));

                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, 0), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, -1), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Secret, -1));

                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Maze, -1), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Feldspar, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Maze, 0), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Maze, 1), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Feldspar, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Maze, 2), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Maze, 3), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Maze, 4), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Maze, 5), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Maze, 6), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Feldspar, 0));
                innerPortalMap.Add(Tuple.Create(HeavenlyBodies.InnerDarkBramble_Maze, 7), Tuple.Create(HeavenlyBodies.InnerDarkBramble_Gutter, 0));

                return Tuple.Create(outerPortalMap, innerPortalMap);
            }
        }

        public static Tuple<Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>, Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>> defaultMapping
        {
            get
            {
                var outerPortalMap = new Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>();
                var innerPortalMap = new Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>();
                foreach (var val in _defaultMapping.Item1)
                {
                    if (val.Key != null && val.Value != null)
                    {
                        outerPortalMap.Add(Tuple.Create(val.Key.Item1, val.Key.Item2), Tuple.Create(val.Value.Item1, val.Value.Item2));
                    }
                }
                foreach (var val in _defaultMapping.Item2)
                {
                    if (val.Key != null && val.Value != null)
                    {
                        innerPortalMap.Add(Tuple.Create(val.Key.Item1, val.Key.Item2), Tuple.Create(val.Value.Item1, val.Value.Item2));
                    }
                }
                return Tuple.Create(outerPortalMap, innerPortalMap);
            }
            set
            {
                if (value.Item1 != null)
                {
                    _defaultMapping.Item1.Clear();
                    foreach (var val in value.Item1)
                    {

                        if (val.Key != null && val.Value != null)
                        {
                            _defaultMapping.Item1.Add(Tuple.Create(val.Key.Item1, val.Key.Item2), Tuple.Create(val.Value.Item1, val.Value.Item2));
                        }
                    }
                }
                if (value.Item2 != null)
                {
                    _defaultMapping.Item1.Clear();
                    foreach (var val in value.Item2)
                    {
                        if (val.Key != null && val.Value != null)
                        {
                            _defaultMapping.Item2.Add(Tuple.Create(val.Key.Item1, val.Key.Item2), Tuple.Create(val.Value.Item1, val.Value.Item2));
                        }
                    }
                }
                enabledManagement = true;
                mapping = Tuple.Create(_outerPortalMap, _innerPortalMap);
            }
        }

        public static Tuple<Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>, Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>> mapping
        {
            get
            {
                var allPortals = portals;
                var outerPortalMap = new Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>();
                var innerPortalMap = new Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>();
                if (allPortals.Count < 1)
                {
                    foreach (var val in _outerPortalMap)
                    {
                        if (val.Key != null && val.Value != null)
                        {
                            outerPortalMap.Add(Tuple.Create(val.Key.Item1, val.Key.Item2), Tuple.Create(val.Value.Item1, val.Value.Item2));
                        }
                    }
                    foreach (var val in _innerPortalMap)
                    {
                        if (val.Key != null && val.Value != null)
                        {
                            innerPortalMap.Add(Tuple.Create(val.Key.Item1, val.Key.Item2), Tuple.Create(val.Value.Item1, val.Value.Item2));
                        }
                    }
                }
                else
                {
                    updateLists();
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
                        _outerPortalMap = new Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>();
                        foreach (var outerMappping in defaults.Item1)
                        {
                            _outerPortalMap[outerMappping.Key] = outerMappping.Value;
                        }
                    }
                    if (_innerPortalMap == null)
                    {
                        _innerPortalMap = new Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>();
                        foreach (var innerMapping in defaults.Item2)
                        {
                            _innerPortalMap[innerMapping.Key] = innerMapping.Value;
                        }
                    }
                }

                foreach (var outerMappping in value.Item1)
                {
                    if (outerMappping.Key != null && outerMappping.Value != null)
                    {
                        _outerPortalMap[Tuple.Create(outerMappping.Key.Item1, outerMappping.Key.Item2)] = Tuple.Create(outerMappping.Value.Item1, outerMappping.Value.Item2);
                    }
                }
                foreach (var innerMapping in value.Item2)
                {
                    if (innerMapping.Key != null && innerMapping.Value != null)
                    {
                        _innerPortalMap[Tuple.Create(innerMapping.Key.Item1, innerMapping.Key.Item2)] = Tuple.Create(innerMapping.Value.Item1, innerMapping.Value.Item2);
                    }
                }

                enabledManagement = true;
                requireUpdate = true;
                updateLists();
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
            _portals[HeavenlyBodies.TimberHearth] = new List<Tuple<FogWarpVolume, float>>();
            _portals[HeavenlyBodies.DarkBramble] = new List<Tuple<FogWarpVolume, float>>();
            _portals[HeavenlyBodies.InnerDarkBramble_Hub] = new List<Tuple<FogWarpVolume, float>>();
            _portals[HeavenlyBodies.InnerDarkBramble_EscapePod] = new List<Tuple<FogWarpVolume, float>>();
            _portals[HeavenlyBodies.InnerDarkBramble_Nest] = new List<Tuple<FogWarpVolume, float>>();
            _portals[HeavenlyBodies.InnerDarkBramble_Feldspar] = new List<Tuple<FogWarpVolume, float>>();
            _portals[HeavenlyBodies.InnerDarkBramble_Gutter] = new List<Tuple<FogWarpVolume, float>>();
            _portals[HeavenlyBodies.InnerDarkBramble_Vessel] = new List<Tuple<FogWarpVolume, float>>();
            _portals[HeavenlyBodies.InnerDarkBramble_Maze] = new List<Tuple<FogWarpVolume, float>>();
            _portals[HeavenlyBodies.InnerDarkBramble_SmallNest] = new List<Tuple<FogWarpVolume, float>>();
            _portals[HeavenlyBodies.InnerDarkBramble_Secret] = new List<Tuple<FogWarpVolume, float>>();
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

        public static void SceneLoaded()
        {
            if (logFrequency > 0)
            {
                var map = mapping;
                Helper.helper.Console.WriteLine($"Scene Loaded Bramble Outer Portal State");
                foreach (var element in map.Item1)
                {
                    Helper.helper.Console.WriteLine($"{element.Key?.Item1 ?? HeavenlyBody.None}:{element.Key?.Item2.ToString() ?? ""} -> {element.Value?.Item1 ?? HeavenlyBody.None}:{element.Value?.Item2.ToString() ?? ""}");
                }

                Helper.helper.Console.WriteLine($"Scene Loaded Bramble Inner Portal State");
                foreach (var element in map.Item2)
                {
                    Helper.helper.Console.WriteLine($"{element.Key?.Item1 ?? HeavenlyBody.None}:{element.Key?.Item2.ToString() ?? ""} -> {element.Value?.Item1 ?? HeavenlyBody.None}:{element.Value?.Item2.ToString() ?? ""}");
                }
            }
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
                    allBodies.Sort((t1, t2) => t1.value.CompareTo(t2.value));

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

            if (logFrequency > 0)
            {
                if (GameTimer.FramesSinceAwake % logFrequency == 0)
                {
                    var map = mapping;
                    Helper.helper.Console.WriteLine($"Scene Loaded Bramble Outer Portal State");
                    foreach (var element in map.Item1)
                    {
                        Helper.helper.Console.WriteLine($"{element.Key?.Item1 ?? HeavenlyBody.None}:{element.Key?.Item2.ToString() ?? ""} -> {element.Value?.Item1 ?? HeavenlyBody.None}:{element.Value?.Item2.ToString() ?? ""}");
                    }

                    Helper.helper.Console.WriteLine($"Scene Loaded Bramble Inner Portal State");
                    foreach (var element in map.Item2)
                    {
                        Helper.helper.Console.WriteLine($"{element.Key?.Item1 ?? HeavenlyBody.None}:{element.Key?.Item2.ToString() ?? ""} -> {element.Value?.Item1 ?? HeavenlyBody.None}:{element.Value?.Item2.ToString() ?? ""}");
                    }
                }
            }
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

        private static string getString(Tuple<HeavenlyBody, int> sender, ref Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>> map)
        {
            Tuple<HeavenlyBody, int> reciever;
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

        public static InnerFogWarpVolume getInnerPortal(HeavenlyBody body, int index)
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

        public static OuterFogWarpVolume getOuterPortal(HeavenlyBody body, int index)
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

        public static Tuple<HeavenlyBody, int> find(FogWarpVolume volume)
        {
            var body = findBody(volume);
            if (body == null
                    || body == HeavenlyBodies.None)
                return null;

            int? index = findIndex(volume, body);
            if (!index.HasValue)
                return null;
            return Tuple.Create(body, index.Value);
        }

        private static HeavenlyBody findBody(FogWarpVolume volume)
        {
            if (volume == null)
            {
                return null;
            }
            if (volume is SecretFogWarpVolume)
            {
                return HeavenlyBodies.InnerDarkBramble_Secret;
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
                    return HeavenlyBodies.InnerDarkBramble_Hub;
                case OuterFogWarpVolume.Name.EscapePod:
                    return HeavenlyBodies.InnerDarkBramble_EscapePod;
                case OuterFogWarpVolume.Name.AnglerNest:
                    return HeavenlyBodies.InnerDarkBramble_Nest;
                case OuterFogWarpVolume.Name.Pioneer:
                    return HeavenlyBodies.InnerDarkBramble_Feldspar;
                case OuterFogWarpVolume.Name.ExitOnly:
                    return HeavenlyBodies.InnerDarkBramble_Gutter;
                case OuterFogWarpVolume.Name.Vessel:
                    return HeavenlyBodies.InnerDarkBramble_Vessel;
                case OuterFogWarpVolume.Name.Cluster:
                    return HeavenlyBodies.InnerDarkBramble_Maze;
                case OuterFogWarpVolume.Name.SmallNest:
                    return HeavenlyBodies.InnerDarkBramble_SmallNest;
                case null:
                default:
                    var state = PositionState.fromCurrentState(volume.gameObject);
                    if (state == null)
                        return null;
                    return getClosest(state.position).Item1;
            }
        }

        private static int? findIndex(FogWarpVolume volume, HeavenlyBody body)
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
                var index = innerPortals.FindAll(x => (x.Item1 == body) && (x.Item2.IsProbeOnly() == volume.IsProbeOnly())).FindIndex(x => x.Item2 == volume);
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

        public static void remapOuterPortal(Tuple<HeavenlyBody, int> start, Tuple<HeavenlyBody, int> end)
        {
            _remapOuterPortal(start, end);
            if (end != null && start != null)
            {
                var map = _outerPortalMap ?? mapping.Item1;
                map[start] = end;
                _outerPortalMap = map;
            }
        }

        public static void remapInnerPortal(Tuple<HeavenlyBody, int> start, Tuple<HeavenlyBody, int> end)
        {
            _remapInnerPortal(start, end);
            if (start != null && end != null)
            {
                var map = _innerPortalMap ?? mapping.Item2;
                map[start] = end;
                _innerPortalMap = map;
            }
        }

        private static void _remapOuterPortal(Tuple<HeavenlyBody, int> start, Tuple<HeavenlyBody, int> end)
        {
            var outerPortal = start == null ? null : getOuterPortal(start.Item1, start.Item2);
            var innerPortal = end == null ? null : getInnerPortal(end.Item1, end.Item2);
            if (innerPortal != null && outerPortal != null)
            {
                outerPortal.SetValue("_linkedInnerWarpVolume", innerPortal);
            }
        }

        private static void _remapInnerPortal(Tuple<HeavenlyBody, int> start, Tuple<HeavenlyBody, int> end)
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
                        Tuple<HeavenlyBody, float> parent;
                        parent = getClosest(state.position);
                        _portals[parent.Item1].Add(Tuple.Create(volume, parent.Item2));
                        volume.OnWarpDetector += (detector) => onWarp(volume, parent.Item1, detector);
                    }
                }
                unprocessedPortals.Clear();

                foreach (HeavenlyBody key in _portals.Keys)
                {
                    var volumes = _portals[key];
                    volumes.RemoveAll(v => v?.Item1 == null || v?.Item1?.gameObject == null);
                    volumes.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                }
                requireUpdate = false;

                if (enabledManagement)
                {
                    doMapping();
                }
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

        private static Tuple<HeavenlyBody, float> getClosest(Vector3 position)
        {
            var keys = new HeavenlyBody[_portals.Count];
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

        private static void onWarp(FogWarpVolume instance, HeavenlyBody portalParent, FogWarpDetector warpedObject)
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

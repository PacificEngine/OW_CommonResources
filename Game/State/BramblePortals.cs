
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PacificEngine.OW_CommonResources.Game.Resource;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Game.State
{
    /*	
InnerFogWarpVolume._linkedOuterWarpVolume
InnerFogWarpVolume._linkedOuterWarpName

InnerFogWarpVolume._containerWarpVolume

OuterFogWarpVolume._linkedInnerWarpVolume
OuterFogWarpVolume._name

InnerFogWarpVolume._senderWarps
*/


    public static class BramblePortals
    {
        public delegate void BrambleWarpEvent(FogWarpDetector.Name warpObject, bool isInnerPortal, Tuple<Position.HeavenlyBodies, int> portal);

        private static int processingFrame = -1;
        private static HashSet<OuterFogWarpVolume> alreadyProcessed = new HashSet<OuterFogWarpVolume>();
        private static List<FogWarpVolume> unprocessedPortals = new List<FogWarpVolume>();
        private static Dictionary<Position.HeavenlyBodies, List<Tuple<FogWarpVolume, float>>> _portals = new Dictionary<Position.HeavenlyBodies, List<Tuple<FogWarpVolume, float>>>();
        private static Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>> _outerPortalMap = null;
        private static Dictionary<Tuple<Position.HeavenlyBodies, int>, Tuple<Position.HeavenlyBodies, int>> _innerPortalMap = null;
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
                    if (_outerPortalMap != null && _innerPortalMap != null)
                    {
                        Tuple.Create(_outerPortalMap, _innerPortalMap);
                    }
                    var defaults = defaultMapping;
                    return Tuple.Create(_outerPortalMap ?? defaultMapping.Item1, _innerPortalMap ?? defaultMapping.Item2);
                }

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
                doMapping();
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
            updateLists();
        }

        public static InnerFogWarpVolume getInnerPortal(Position.HeavenlyBodies body, int index)
        {
            var inner = innerPortals;
            return getInnerPortal(ref inner, body, index);
        }

        public static OuterFogWarpVolume getOuterPortal(Position.HeavenlyBodies body, int index)
        {
            var outer = outerPortals;
            var secret = secretPortals;
            return getOuterPortal(ref outer, ref secret, body, index);
        }

        public static List<InnerFogWarpVolume> getInnerPortals(OuterFogWarpVolume outer)
        {
            return outer.GetValue<List<InnerFogWarpVolume>>("_senderWarps");
        }

        public static Tuple<Position.HeavenlyBodies, int> find(FogWarpVolume volume)
        {
            Position.HeavenlyBodies body = findBody(volume);
            int? index = findIndex(volume, body);

            if (index.HasValue)
                return Tuple.Create(body, index.Value);
            return null;
        }

        private static Position.HeavenlyBodies findBody(FogWarpVolume volume)
        {
            if (volume is SecretFogWarpVolume)
            {
                return Position.HeavenlyBodies.InnerDarkBramble_Secret;
            }
            switch (volume.GetAttachedOWRigidbody()?.GetComponentInChildren<OuterFogWarpVolume>()?.GetName())
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
                    return getClosest(volume.transform.position)[0].Item1;
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

        public static void remapOuterPortal(Tuple<Position.HeavenlyBodies, int> outer, Tuple<Position.HeavenlyBodies, int> inner)
        {
            remapOuterPortal(getOuterPortal(outer.Item1, outer.Item2), getInnerPortal(inner.Item1, inner.Item2));
        }

        public static void remapInnerPortal(Tuple<Position.HeavenlyBodies, int> outer, Tuple<Position.HeavenlyBodies, int> inner)
        {
            remapInnerPortal(getOuterPortal(outer.Item1, outer.Item2), getInnerPortal(inner.Item1, inner.Item2));
        }

        private static InnerFogWarpVolume getInnerPortal(ref List<Tuple<Position.HeavenlyBodies, InnerFogWarpVolume>> inner, Position.HeavenlyBodies body, int index)
        {
            if (index < 0)
            {
                return inner.FindAll(x => x.Item1 == body && x.Item2.IsProbeOnly()).ElementAtOrDefault((-1 * index) - 1)?.Item2;
            }
            return inner.FindAll(x => x.Item1 == body && !x.Item2.IsProbeOnly()).ElementAtOrDefault(index)?.Item2;
        }

        private static OuterFogWarpVolume getOuterPortal(ref List<Tuple<Position.HeavenlyBodies, OuterFogWarpVolume>> outer, ref List<Tuple<Position.HeavenlyBodies, SecretFogWarpVolume>> secret, Position.HeavenlyBodies body, int index)
        {
            if (index < 0)
            {
                return secret.FindAll(x => x.Item1 == body).ElementAtOrDefault((-1 * index) - 1)?.Item2;
            }
            return outer.FindAll(x => x.Item1 == body).ElementAtOrDefault(index)?.Item2;
        }

        private static void remapOuterPortal(OuterFogWarpVolume outer, InnerFogWarpVolume inner)
        {
            if (inner != null && outer != null)
            {
                outer.SetValue("_linkedInnerWarpVolume", inner);
            }
        }

        private static void remapInnerPortal(OuterFogWarpVolume outer, InnerFogWarpVolume inner)
        {
            if (inner != null && outer != null)
            {
                inner.GetValue<OuterFogWarpVolume>("_linkedOuterWarpVolume").GetValue<List<InnerFogWarpVolume>>("_senderWarps").Remove(inner);
                inner.SetValue("_linkedOuterWarpVolume", outer);
                inner.SetValue("_linkedOuterWarpName", outer.GetName());
                outer.RegisterSenderWarp(inner);
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
                        Tuple<Position.HeavenlyBodies, float> parent;
                        parent = getClosest(volume.transform.position)[0];
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
            var currentMapping = Tuple.Create(_outerPortalMap ?? defaultMapping.Item1, _innerPortalMap ?? defaultMapping.Item2);
            var outer = outerPortals;
            var inner = innerPortals;
            var secret = secretPortals;

            foreach (var outerMapping in currentMapping.Item1)
            {
                var outerPortal = getOuterPortal(ref outer, ref secret, outerMapping.Key.Item1, outerMapping.Key.Item2);
                var innerPortal = getInnerPortal(ref inner, outerMapping.Value.Item1, outerMapping.Value.Item2);
                remapOuterPortal(outerPortal, innerPortal);
            }

            foreach (var innerMapping in currentMapping.Item2)
            {
                var innerPortal = getInnerPortal(ref inner, innerMapping.Key.Item1, innerMapping.Key.Item2);
                var outerPortal = getOuterPortal(ref outer, ref secret, innerMapping.Value.Item1, innerMapping.Value.Item2);
                remapInnerPortal(outerPortal, innerPortal);
            }
        }

        private static List<Tuple<Position.HeavenlyBodies, float>> getClosest(Vector3 position)
        {
            var keys = new Position.HeavenlyBodies[_portals.Count];
            _portals.Keys.CopyTo(keys, 0);
            return Position.getClosest(position, keys);
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
            var index = findIndex(instance, portalParent);
            if (index.HasValue)
                onBrambleWarp?.Invoke(warpedObject.GetValue<FogWarpDetector.Name>("_name"), instance is InnerFogWarpVolume, Tuple.Create(portalParent, index.Value));
        }
    }
}

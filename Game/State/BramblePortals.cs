
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
        public delegate void BrambleWarpEvent(FogWarpDetector.Name warpObject, Position.HeavenlyBodies portalParent, FogWarpVolume portal);

        private static int processingFrame = -1;
        private static HashSet<OuterFogWarpVolume> alreadyProcessed = new HashSet<OuterFogWarpVolume>();
        private static List<FogWarpVolume> unprocessedPortals = new List<FogWarpVolume>();
        private static Dictionary<Position.HeavenlyBodies, List<Tuple<FogWarpVolume, float>>> portals = new Dictionary<Position.HeavenlyBodies, List<Tuple<FogWarpVolume, float>>>();
        private static Boolean requireUpdate = false;


        public static event BrambleWarpEvent onBrambleWarp;

        public static void Start()
        {
            processingFrame = -1;
            alreadyProcessed.Clear();
            unprocessedPortals.Clear();
            portals.Clear();
            requireUpdate = false;

            Helper.helper.HarmonyHelper.AddPostfix<FogWarpVolume>("Awake", typeof(BramblePortals), "onFogWarpVolumeAwake");
            Helper.helper.HarmonyHelper.AddPrefix<OuterFogWarpVolume>("PropagateCanvasMarkerOutwards", typeof(BramblePortals), "onOuterFogWarpVolumePropagateCanvasMarkerOutwards");

            portals[Position.HeavenlyBodies.TimberHearth] = new List<Tuple<FogWarpVolume, float>>();
            portals[Position.HeavenlyBodies.DarkBramble] = new List<Tuple<FogWarpVolume, float>>();
            portals[Position.HeavenlyBodies.InnerDarkBramble_Hub] = new List<Tuple<FogWarpVolume, float>>();
            portals[Position.HeavenlyBodies.InnerDarkBramble_EscapePod] = new List<Tuple<FogWarpVolume, float>>();
            portals[Position.HeavenlyBodies.InnerDarkBramble_Nest] = new List<Tuple<FogWarpVolume, float>>();
            portals[Position.HeavenlyBodies.InnerDarkBramble_Feldspar] = new List<Tuple<FogWarpVolume, float>>();
            portals[Position.HeavenlyBodies.InnerDarkBramble_Gutter] = new List<Tuple<FogWarpVolume, float>>();
            portals[Position.HeavenlyBodies.InnerDarkBramble_Vessel] = new List<Tuple<FogWarpVolume, float>>();
            portals[Position.HeavenlyBodies.InnerDarkBramble_Maze] = new List<Tuple<FogWarpVolume, float>>();
            portals[Position.HeavenlyBodies.InnerDarkBramble_SmallNest] = new List<Tuple<FogWarpVolume, float>>();
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

        public static void remapOuterPortal(OuterFogWarpVolume outer, InnerFogWarpVolume inner)
        {
            outer.SetValue("_linkedInnerWarpVolume", inner);
        }

        public static void remapInnerPortal(OuterFogWarpVolume outer, InnerFogWarpVolume inner)
        {
            inner.GetValue<OuterFogWarpVolume>("_linkedOuterWarpVolume").GetValue<List<InnerFogWarpVolume>>("_senderWarps").Remove(inner);
            inner.SetValue("_linkedOuterWarpVolume", outer);
            inner.SetValue("_linkedOuterWarpName", outer.GetName());
            outer.RegisterSenderWarp(inner);
        }


        public static Position.HeavenlyBodies findBody(FogWarpVolume volume)
        {
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

        public static List<InnerFogWarpVolume> getInnerPortals(OuterFogWarpVolume outer)
        {
            return outer.GetValue<List<InnerFogWarpVolume>>("_senderWarps");
        }

        public static List<Position.HeavenlyBodies> getBodies()
        {
            updateLists();
            return new List<Position.HeavenlyBodies>(portals.Keys);
        }

        public static List<Tuple<Position.HeavenlyBodies, SecretFogWarpVolume>> getSecretVolume()
        {
            updateLists();
            var obj = new List<Tuple<Position.HeavenlyBodies, SecretFogWarpVolume>>();
            foreach (var key in portals.Keys)
            {
                obj.AddRange(portals[key].FindAll(x => x.Item1 is SecretFogWarpVolume).ConvertAll(x => Tuple.Create(key, x.Item1 as SecretFogWarpVolume)));
            }
            return obj;
        }

        public static List<Tuple<Position.HeavenlyBodies, OuterFogWarpVolume>> getOuterVolumes()
        {
            updateLists();
            var obj = new List<Tuple<Position.HeavenlyBodies, OuterFogWarpVolume>> ();
            foreach(var key in portals.Keys)
            {
                obj.AddRange(portals[key].FindAll(x => (x.Item1 is OuterFogWarpVolume) && !(x.Item1 is SecretFogWarpVolume)).ConvertAll(x => Tuple.Create(key, x.Item1 as OuterFogWarpVolume)));
            }
            return obj;
        }

        public static List<Tuple<Position.HeavenlyBodies, OuterFogWarpVolume>> getOuterVolumes(Position.HeavenlyBodies body)
        {
            updateLists();
            return portals[body].FindAll(x => (x.Item1 is OuterFogWarpVolume) && !(x.Item1 is SecretFogWarpVolume)).ConvertAll(x => Tuple.Create(body, x.Item1 as OuterFogWarpVolume));
        }

        public static List<Tuple<Position.HeavenlyBodies, InnerFogWarpVolume>> getInnerVolumes()
        {
            updateLists();
            var obj = new List<Tuple<Position.HeavenlyBodies, InnerFogWarpVolume>>();
            foreach (var key in portals.Keys)
            {
                obj.AddRange(portals[key].FindAll(x => x.Item1 is InnerFogWarpVolume).ConvertAll(x => Tuple.Create(key, x.Item1 as InnerFogWarpVolume)));
            }
            return obj;
        }

        public static List<Tuple<Position.HeavenlyBodies, InnerFogWarpVolume>> getInnerVolumes(Position.HeavenlyBodies body)
        {
            updateLists();
            return portals[body].FindAll(x => x.Item1 is InnerFogWarpVolume).ConvertAll(x => Tuple.Create(body, x.Item1 as InnerFogWarpVolume));
        }

        public static List<Tuple<Position.HeavenlyBodies, CapsuleFogWarpVolume>> getCapsuleVolumes()
        {
            updateLists();
            var obj = new List<Tuple<Position.HeavenlyBodies, CapsuleFogWarpVolume>>();
            foreach (var key in portals.Keys)
            {
                obj.AddRange(portals[key].FindAll(x => x.Item1 is CapsuleFogWarpVolume).ConvertAll(x => Tuple.Create(key, x.Item1 as CapsuleFogWarpVolume)));
            }
            return obj;
        }

        public static List<Tuple<Position.HeavenlyBodies, CapsuleFogWarpVolume>> getCapsuleVolumes(Position.HeavenlyBodies body)
        {
            updateLists();
            return portals[body].FindAll(x => x.Item1 is CapsuleFogWarpVolume).ConvertAll(x => Tuple.Create(body, x.Item1 as CapsuleFogWarpVolume));
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
                        portals[parent.Item1].Add(Tuple.Create(volume, parent.Item2));
                        volume.OnWarpDetector += (detector) => onWarp(volume, parent.Item1, detector);
                    }
                }
                unprocessedPortals.Clear();

                foreach (Position.HeavenlyBodies key in portals.Keys)
                {
                    var volumes = portals[key];
                    volumes.RemoveAll(v => v?.Item1 == null || v?.Item1?.gameObject == null);
                    volumes.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                }
                requireUpdate = false;
            }
        }

        private static List<Tuple<Position.HeavenlyBodies, float>> getClosest(Vector3 position)
        {
            var keys = new Position.HeavenlyBodies[portals.Count];
            portals.Keys.CopyTo(keys, 0);
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
            onBrambleWarp?.Invoke(warpedObject.GetValue<FogWarpDetector.Name>("_name"), portalParent, instance);
        }
    }
}

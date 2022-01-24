
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game.Display;
using PacificEngine.OW_CommonResources.Game.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Game.State
{
    public static class WarpPad
    {
        private const string classId = "PacificEngine.OW_CommonResources.Game.State.WarpPad";

        private static float _lastUpdate = 0f;
        private static List<string> debugIds = new List<string>();
        public static bool _enabledManagement { get; set; } = false;
        public static bool enabledManagement { get { return _enabledManagement; } set { _enabledManagement = value; requireUpdate = true; } }
        public static bool debugMode { get; set; } = false;

        public delegate void PadWarpEvent(OWRigidbody warpObject, Tuple<HeavenlyBody, int> sender, Tuple<HeavenlyBody, int> reciever);

        private static bool requireUpdate = false;
        private static List<NomaiWarpPlatform> unprocessedPortals = new List<NomaiWarpPlatform>();
        private static Dictionary<HeavenlyBody, List<Tuple<NomaiWarpPlatform, float>>> _portals = new Dictionary<HeavenlyBody, List<Tuple<NomaiWarpPlatform, float>>>();
        private static Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>> _defaultMapping = standardMapping;
        private static Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>> _mapping = standardMapping;

        public static List<HeavenlyBody> bodies
        {
            get
            {
                updateLists();
                return new List<HeavenlyBody>(_portals.Keys);
            }
        }

        public static List<Tuple<HeavenlyBody, NomaiWarpPlatform>> portals
        {
            get
            {
                updateLists();
                var obj = new List<Tuple<HeavenlyBody, NomaiWarpPlatform>>();
                foreach (var key in _portals.Keys)
                {
                    obj.AddRange(_portals[key].ConvertAll(x => Tuple.Create(key, x.Item1)));
                }
                return obj;
            }
        }

        public static List<Tuple<HeavenlyBody, NomaiWarpTransmitter>> senderPortals
        {
            get
            {
                return portals.FindAll(x => (x.Item2 is NomaiWarpTransmitter)).ConvertAll(x => Tuple.Create(x.Item1, x.Item2 as NomaiWarpTransmitter));
            }
        }

        public static List<Tuple<HeavenlyBody, NomaiWarpReceiver>> recieverPortals
        {
            get
            {
                return portals.FindAll(x => x.Item2 is NomaiWarpReceiver).ConvertAll(x => Tuple.Create(x.Item1, x.Item2 as NomaiWarpReceiver));
            }
        }

        public static Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>> standardMapping
        {
            get
            {
                var mapping = new Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>();
                mapping.Add(Tuple.Create(HeavenlyBodies.AshTwin, 0), Tuple.Create(HeavenlyBodies.GiantsDeep, -1));
                mapping.Add(Tuple.Create(HeavenlyBodies.AshTwin, 1), Tuple.Create(HeavenlyBodies.BrittleHollow, -1));
                mapping.Add(Tuple.Create(HeavenlyBodies.AshTwin, 2), Tuple.Create(HeavenlyBodies.TimberHearth, -1));
                mapping.Add(Tuple.Create(HeavenlyBodies.AshTwin, 3), Tuple.Create(HeavenlyBodies.EmberTwin, -1));
                mapping.Add(Tuple.Create(HeavenlyBodies.AshTwin, 4), Tuple.Create(HeavenlyBodies.AshTwin, -1));
                mapping.Add(Tuple.Create(HeavenlyBodies.AshTwin, 5), Tuple.Create(HeavenlyBodies.SunStation, -1));
                mapping.Add(Tuple.Create(HeavenlyBodies.WhiteHoleStation, 0), Tuple.Create(HeavenlyBodies.BrittleHollow, -2));

                return mapping;
            }
        }

        public static Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>> defaultMapping
        {
            get
            {
                var value = new Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>();
                foreach (var val in _defaultMapping)
                {
                    if (val.Key != null && val.Value != null)
                    {
                        value.Add(Tuple.Create(val.Key.Item1, val.Key.Item2), Tuple.Create(val.Value.Item1, val.Value.Item2));
                    }
                }
                return value;
            }
            set
            {
                _defaultMapping.Clear();
                foreach (var val in value)
                {
                    if (val.Key != null && val.Value != null)
                    {
                        _defaultMapping.Add(Tuple.Create(val.Key.Item1, val.Key.Item2), Tuple.Create(val.Value.Item1, val.Value.Item2));
                    }
                }
                enabledManagement = true;
                mapping = _mapping;
            }
        }

        public static Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>> mapping
        {
            get
            {
                var allPortals = portals;
                if (allPortals.Count < 1)
                {
                    var value = new Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>();
                    foreach (var val in _mapping)
                    {
                        if (val.Key != null && val.Value != null)
                        {
                            value.Add(Tuple.Create(val.Key.Item1, val.Key.Item2), Tuple.Create(val.Value.Item1, val.Value.Item2));
                        }
                    }
                    return value;
                }

                updateLists();
                var mapping = new Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>>();
                foreach(var portal in allPortals)
                {
                    var index = findIndex(portal.Item2, portal.Item1);
                    if (index.HasValue)
                    {
                        if (portal.Item2 is NomaiWarpReceiver)
                        {
                            var linked = portal.Item2.GetValue<NomaiWarpPlatform>("_returnPlatform");
                            if (linked != null)
                            {
                                mapping[Tuple.Create(portal.Item1, index.Value)] = find(linked);
                            }
                        }
                        else
                        {
                            var linked = portal.Item2.GetValue<NomaiWarpPlatform>("_targetReceiver");
                            if (linked != null)
                            {
                                mapping[Tuple.Create(portal.Item1, index.Value)] = find(linked);
                            }
                            else
                            {
                                Tuple<HeavenlyBody, int> target;
                                if (_mapping.TryGetValue(Tuple.Create(portal.Item1, index.Value), out target))
                                {
                                    mapping[Tuple.Create(portal.Item1, index.Value)] = target;
                                }
                            }
                        }
                    }
                }

                return mapping;
            }
            set
            {
                _mapping.Clear();
                foreach (var val in value)
                {
                    if (val.Key != null && val.Value != null)
                    {
                        _mapping.Add(Tuple.Create(val.Key.Item1, val.Key.Item2), Tuple.Create(val.Value.Item1, val.Value.Item2));
                    }
                }

                enabledManagement = true;
                requireUpdate = true;
                updateLists();
            }
        }

        public static event PadWarpEvent onPadWarp;

        private static void reset()
        {
            unprocessedPortals.Clear();
            requireUpdate = false;

            _portals.Clear();
            _portals[HeavenlyBodies.SunStation] = new List<Tuple<NomaiWarpPlatform, float>>();
            _portals[HeavenlyBodies.AshTwin] = new List<Tuple<NomaiWarpPlatform, float>>();
            _portals[HeavenlyBodies.EmberTwin] = new List<Tuple<NomaiWarpPlatform, float>>();
            _portals[HeavenlyBodies.TimberHearth] = new List<Tuple<NomaiWarpPlatform, float>>();
            _portals[HeavenlyBodies.BrittleHollow] = new List<Tuple<NomaiWarpPlatform, float>>();
            _portals[HeavenlyBodies.GiantsDeep] = new List<Tuple<NomaiWarpPlatform, float>>();
            _portals[HeavenlyBodies.WhiteHoleStation] = new List<Tuple<NomaiWarpPlatform, float>>();
            _portals[HeavenlyBodies.InnerDarkBramble_Vessel] = new List<Tuple<NomaiWarpPlatform, float>>();
            _portals[HeavenlyBodies.EyeOfTheUniverse] = new List<Tuple<NomaiWarpPlatform, float>>();
            _portals[HeavenlyBodies.EyeOfTheUniverse_Vessel] = new List<Tuple<NomaiWarpPlatform, float>>();
        }

        public static void Start()
        {
            reset();

            Helper.helper.HarmonyHelper.AddPostfix<NomaiWarpPlatform>("Awake", typeof(WarpPad), "onNomaiWarpPlatformAwake");
            Helper.helper.HarmonyHelper.AddPostfix<NomaiWarpTransmitter>("FixedUpdate", typeof(WarpPad), "onNomaiWarpTransmitterFixedUpdate");
            Helper.helper.HarmonyHelper.AddPrefix<NomaiWarpTransmitter>("GetViewAngleToTarget", typeof(WarpPad), "onNomaiWarpTransmitterGetViewAngleToTarget");
            Helper.helper.HarmonyHelper.AddPostfix<NomaiWarpReceiver>("ReceiveWarpedBody", typeof(WarpPad), "onNomaiWarpReceiverReceiveWarpedBody");
            Helper.helper.HarmonyHelper.AddPostfix<NomaiWarpReceiver>("OnEntry", typeof(WarpPad), "onNomaiWarpReceiverOnEntry");
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
                    float index = 12.1f;
                    console.setElement(getId("Header"), "Warp Pads", 12.09f);
                    var allBodies = bodies;
                    allBodies.Sort((t1, t2) => t1.value.CompareTo(t2.value));
                    foreach (var body in allBodies)
                    {
                        List<Tuple<NomaiWarpPlatform, float>> portals;
                        if (_portals.TryGetValue(body, out portals))
                        {
                            foreach (var portal in portals)
                            {
                                var portalIndex = findIndex(portal.Item1, body);
                                if (portalIndex.HasValue)
                                {
                                    var portalValue = Tuple.Create(body, portalIndex.Value);
                                    console.setElement(getId(portalValue.Item1 + "." + portalValue.Item2), getString(portalValue, ref map), index);
                                    index += 0.01f;
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

        private static string getString(Tuple<HeavenlyBody, int> sender, ref Dictionary<Tuple<HeavenlyBody, int>, Tuple<HeavenlyBody, int>> map)
        {
            Tuple<HeavenlyBody, int> reciever;
            if (map.TryGetValue(sender, out reciever))
            {
                if (reciever != null)
                {
                    return $"{sender.Item1}:{sender.Item2} -> {reciever.Item1}:{reciever.Item2}";
                }
                else
                {
                    return $"{sender.Item1}:{sender.Item2} -> Nowhere";
                }
            }
            else
            {
                return $"{sender.Item1}:{sender.Item2} -> Nowhere";
            }
        }

        public static Tuple<HeavenlyBody, int> find(NomaiWarpPlatform volume)
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

        public static NomaiWarpPlatform getPlatform(HeavenlyBody body, int index)
        {
            List<Tuple<NomaiWarpPlatform, float>> platform;
            if (_portals.TryGetValue(body, out platform))
            {
                if (index < 0)
                {
                    var element = platform.FindAll(x => x.Item1 is NomaiWarpReceiver).ElementAtOrDefault(-1 * index - 1);
                    return element == null ? null : element.Item1;
                }
                else
                {
                    var element = platform.FindAll(x => !(x.Item1 is NomaiWarpReceiver)).ElementAtOrDefault(index);
                    return element == null ? null : element.Item1;
                }
            }
            return null;
        }

        private static HeavenlyBody findBody(NomaiWarpPlatform volume)
        {
            var state = PositionState.fromCurrentState(volume?.GetAttachedOWRigidbody());
            if (state == null)
                return null;
            return getClosest(state.position).Item1;
        }

        private static int? findIndex(NomaiWarpPlatform volume, HeavenlyBody body)
        {
            var index = portals.FindAll(x => x.Item1 == body && (volume is NomaiWarpReceiver == x.Item2 is NomaiWarpReceiver)).FindIndex(x => x.Item2 == volume);
            if (index < 0)
            {
                return null;
            }
            return volume is NomaiWarpReceiver ? ((-1 * index) - 1) : index;
        }

        public static void remapPad(Tuple<HeavenlyBody, int> sender, Tuple<HeavenlyBody, int> reciever)
        {
            _remapPad(sender, reciever);
            if (sender != null)
            {
                var map = _mapping ?? mapping;
                if (reciever != null)
                {
                    map[sender] = reciever;
                }
                _mapping = map;
            }
        }

        private static void _remapPad(Tuple<HeavenlyBody, int> sender, Tuple<HeavenlyBody, int> reciever)
        {
            var senderPlatform = sender == null ? null : getPlatform(sender.Item1, sender.Item2);
            var recieverPlatform = reciever == null ? null : getPlatform(reciever.Item1, reciever.Item2);
            if (senderPlatform != null)
            {
                if (senderPlatform is NomaiWarpReceiver)
                {
                    if (recieverPlatform != null)
                    {
                        senderPlatform.SetValue("_returnPlatform", recieverPlatform);
                        senderPlatform.SetValue("_waitToActivateReturnWarp", false);
                        senderPlatform.SetValue("_returnOnEntry", true);
                        senderPlatform.enabled = true;
                    }
                    else
                    {
                        senderPlatform.SetValue("_returnPlatform", null);
                        senderPlatform.SetValue("_waitToActivateReturnWarp", true);
                        senderPlatform.SetValue("_returnOnEntry", false);
                        senderPlatform.enabled = false;
                    }
                }
                else
                {
                    if (recieverPlatform != null && recieverPlatform is NomaiWarpReceiver)
                    {
                        senderPlatform.SetValue("_targetReceiver", recieverPlatform);
                    }
                    else
                    {
                        senderPlatform.SetValue("_targetReceiver", null);
                    }
                }
            }
        }

        private static void updateLists()
        {
            if (requireUpdate)
            {
                foreach (var volume in unprocessedPortals)
                {
                    if (volume != null && volume?.gameObject != null)
                    {
                        Tuple<HeavenlyBody, float> parent;
                        var state = PositionState.fromCurrentState(volume.gameObject);
                        parent = getClosest(state.position);
                        if (!_portals.ContainsKey(parent.Item1))
                        {
                            _portals.Add(parent.Item1, new List<Tuple<NomaiWarpPlatform, float>>());
                        }
                        _portals[parent.Item1].Add(Tuple.Create(volume, parent.Item2));
                        volume.OnReceiveWarpedBody += (warpedObject, start, target) => onWarp(volume, parent.Item1, warpedObject, start, target);
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

        private static Tuple<HeavenlyBody, float> getClosest(Vector3 position)
        {
            var keys = new HeavenlyBody[_portals.Count];
            _portals.Keys.CopyTo(keys, 0);
            return Position.getClosest(position, keys)[0];
        }

        private static void doMapping()
        {
            var map = _mapping;
            foreach (var portal in portals)
            {
                var index = findIndex(portal.Item2, portal.Item1);
                if (index.HasValue)
                {
                    var sender = Tuple.Create(portal.Item1, index.Value);
                    Tuple<HeavenlyBody, int> linked;
                    if (map.TryGetValue(sender, out linked))
                    {
                        _remapPad(sender, linked);
                    }
                    else
                    {
                        _remapPad(sender, null);
                    }
                }
            }
        }

        private static void onNomaiWarpPlatformAwake(ref NomaiWarpPlatform __instance)
        {
            unprocessedPortals.Add(__instance);
            requireUpdate = true;
        }

        private static void onNomaiWarpTransmitterFixedUpdate(ref NomaiWarpTransmitter __instance)
        {
            var sender = find(__instance);

            if (__instance.GetValue<NomaiWarpReceiver>("_targetReceiver") == null)
            {
                Tuple<HeavenlyBody, int> target;
                if (_mapping.TryGetValue(sender, out target))
                {
                    var targetPlatform = getPlatform(target.Item1, target.Item2);
                    if (targetPlatform != null)
                    {
                        Transform alignmentTarget;
                        if (targetPlatform is NomaiWarpReceiver)
                        {
                            alignmentTarget = ((NomaiWarpReceiver)targetPlatform).GetAlignmentTarget();
                        }
                        else
                        {
                            alignmentTarget = (targetPlatform.GetAttachedOWRigidbody() == __instance.GetAttachedOWRigidbody() ? Position.getBody(HeavenlyBodies.EmberTwin) : targetPlatform.GetAttachedOWRigidbody()).transform;
                        }
                        var angle = Vector3.Angle(alignmentTarget.position - __instance.transform.position, __instance.GetValue<bool>("_upsideDown") ? (-1f * __instance.transform.up) : __instance.transform.up);
                        if (__instance.GetValue<List<OWRigidbody>>("_objectsOnPlatform").Count <= 0 || __instance.IsBlackHoleOpen() || angle > 0.5f * (TimeLoop.GetSecondsRemaining() < 30.0f ? 5.0f : (double)__instance.GetValue<float>("_alignmentWindow")))
                            return;

                        __instance.OpenBlackHole(targetPlatform);
                    }
                }
            }
        }

        private static bool onNomaiWarpTransmitterGetViewAngleToTarget(ref NomaiWarpPlatform __instance, ref float __result)
        {
            if (__instance.GetValue<NomaiWarpReceiver>("_targetReceiver") != null)
            {
                return true;
            }

            __result = 180f;
            return false;
        }

        private static void onNomaiWarpReceiverReceiveWarpedBody(ref NomaiWarpReceiver __instance)
        {
            var linked = find(__instance);
            Tuple<HeavenlyBody, int> original;
            if (_mapping.TryGetValue(linked, out original))
            {
                var originalPlatform = original == null ? null : getPlatform(original.Item1, original.Item2);
                if (originalPlatform != null)
                {
                    __instance.SetValue("_returnPlatform", originalPlatform);
                }
            }
        }

        private static void onNomaiWarpReceiverOnEntry(ref NomaiWarpReceiver __instance)
        {
            var linked = find(__instance);
            Tuple<HeavenlyBody, int> original;
            if (_mapping.TryGetValue(linked, out original))
            {
                var originalPlatform = original == null ? null : getPlatform(original.Item1, original.Item2);
                if (originalPlatform != null)
                {
                    __instance.SetValue("_returnPlatform", originalPlatform);
                }
            }
        }

        private static void onWarp(NomaiWarpPlatform caller, HeavenlyBody parentBody, OWRigidbody warpedBody, NomaiWarpPlatform startPlatform, NomaiWarpPlatform targetPlatform)
        {
            if (caller == startPlatform)
            {
                var startIndex = findIndex(startPlatform, parentBody);
                if (startIndex.HasValue)
                {
                    onPadWarp?.Invoke(warpedBody, Tuple.Create(parentBody, startIndex.Value), find(targetPlatform));
                }
            }
            else
            {
                var targetIndex = findIndex(targetPlatform, parentBody);
                if (targetIndex.HasValue)
                {
                    onPadWarp?.Invoke(warpedBody, find(startPlatform), Tuple.Create(parentBody, targetIndex.Value));
                }
            }

            if (targetPlatform is NomaiWarpReceiver)
            {
                var linked = find(targetPlatform);
                Tuple<HeavenlyBody, int> original;
                if (_mapping.TryGetValue(linked, out original))
                {
                    var originalPlatform = original == null ? null : getPlatform(original.Item1, original.Item2);
                    if (originalPlatform != null)
                    {
                        targetPlatform.SetValue("_returnPlatform", originalPlatform);
                    }
                }
            }
        }
    }
}

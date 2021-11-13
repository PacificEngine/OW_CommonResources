using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PacificEngine.OW_CommonResources.Game.Resource;


namespace PacificEngine.OW_CommonResources.Game.State
{
    public static class Anglerfish
    {
        private static HashSet<AnglerfishController> anglerfish = new HashSet<AnglerfishController>();
        private static HashSet<AnglerfishController> createdAnglerfish = new HashSet<AnglerfishController>();
        private static Mesh anglerfishSkin = null;

        private static bool? _enabledAI = null;
        private static bool _canStun = true;
        private static bool _canFeel = true;
        private static bool _canHear = true;
        private static bool _canSmell = false;
        private static bool _canSee = false;
        private static float? _overrideAcceleration = null;
        private static float? _overrideInvestigateSpeed = null;
        private static float? _overrideChaseSpeed = null;
        private static float? _overrideTurnSpeed = null;
        private static float? _overrideQuickTurnSpeed = null;
        private static float? _overrideMouthOpenDistance = null;
        private static float? _overridePursueDistance = null;
        private static float? _overrideEscapeDistance = null;

        private static float _visionX = 180f;
        private static float _visionY = 100f;
        private static float _visionYoffset = 45f;
        private static float _visionDistance = 200f;
        private static float _smellDistance = 500f;

        private static float _lastHardUpdate = 0f;

        public static bool? enabledAI { get { return _enabledAI.GetValueOrDefault(true); } set { _enabledAI = value; updateAnglerfish(); } }
        public static bool canStun { get { return _canStun; } set { _canStun = value; } }
        public static bool canFeel { get { return _canFeel; } set { _canFeel = value; } }
        public static bool canHear { get { return _canHear; } set { _canHear = value; } }
        public static bool canSmell { get { return _canSmell; } set { _canSmell = value; } }
        public static bool canSee { get { return _canSee; } set { _canSee = value; } }
        public static float? acceleration { get { return _overrideAcceleration.GetValueOrDefault(getParameter("_acceleration", 2f)); } set { _overrideAcceleration = value; updateParameter("_acceleration", value, 2f); } }
        public static float? investigateSpeed { get { return _overrideInvestigateSpeed.GetValueOrDefault(getParameter("_investigateSpeed", 20f)); } set { _overrideInvestigateSpeed = value; updateParameter("_investigateSpeed", value, 20f); } }
        public static float? chaseSpeed { get { return _overrideChaseSpeed.GetValueOrDefault(getParameter("_chaseSpeed", 42f)); } set { _overrideChaseSpeed = value; updateParameter("_chaseSpeed", value, 42f); } }
        public static float? turnSpeed { get { return _overrideTurnSpeed.GetValueOrDefault(getParameter("_turnSpeed", 180f)); } set { _overrideTurnSpeed = value; updateParameter("_turnSpeed", value, 180f); } }
        public static float? quickTurnSpeed { get { return _overrideQuickTurnSpeed.GetValueOrDefault(getParameter("_quickTurnSpeed", 360f)); } set { _overrideQuickTurnSpeed = value; updateParameter("_quickTurnSpeed", value, 360f); } }
        public static float? mouthOpenDistance { get { return _overrideMouthOpenDistance.GetValueOrDefault(getParameter("_arrivalDistance", 100f)); } set { _overrideMouthOpenDistance = value; updateParameter("_arrivalDistance", value, 100f); } }
        public static float? pursueDistance { get { return _overridePursueDistance.GetValueOrDefault(getParameter("_pursueDistance", 200f)); } set { _overridePursueDistance = value; updateParameter("_pursueDistance", value, 200f); } }
        public static float? escapeDistance { get { return _overrideEscapeDistance.GetValueOrDefault(getParameter("_escapeDistance", 500f)); } set { _overrideEscapeDistance = value; updateParameter("_escapeDistance", value, 500f); } }

        public static float? visionX { get { return _visionX; } set { _visionX = value.GetValueOrDefault(180f); } }
        public static float? visionY { get { return _visionY; } set { _visionY = value.GetValueOrDefault(100f); } }
        public static float? visionYoffset { get { return _visionYoffset; } set { _visionYoffset = value.GetValueOrDefault(45f); } }
        public static float? visionDistance { get { return _visionDistance; } set { _visionDistance = value.GetValueOrDefault(200f); } }
        public static float? smellDistance { get { return _smellDistance; } set { _smellDistance = value.GetValueOrDefault(500f); } }

        public static void Start()
        {
            Helper.helper.HarmonyHelper.AddPrefix<AnglerfishController>("Awake", typeof(Anglerfish), "AnglerfishControllerAwake");
            Helper.helper.HarmonyHelper.AddPrefix<AnglerfishController>("OnDestroy", typeof(Anglerfish), "AnglerfishControllerOnDestroy");
            Helper.helper.HarmonyHelper.AddPrefix<AnglerfishController>("OnImpact", typeof(Anglerfish), "AnglerfishControllerOnFeel");
            Helper.helper.HarmonyHelper.AddPrefix<AnglerfishController>("OnClosestAudibleNoise", typeof(Anglerfish), "AnglerfishControllerOnHearSound");
            Helper.helper.HarmonyHelper.AddPrefix<AnglerfishController>("MoveTowardsTarget", typeof(Anglerfish), "AnglerfishControllerMoveTowardsTarget");
        }

        public static void Awake()
        {

        }

        public static void Destroy()
        {
        }

        public static void Update()
        {
            if (createdAnglerfish.Count == 0 && anglerfish.Count > 0)
            {
                var i = anglerfish.GetEnumerator();
                i.MoveNext();
                createAnglerfish(i.Current);
            }

            updateAnglerfish();
            hardUpdateAnglerfish();
        }

        private static bool AnglerfishControllerAwake(ref AnglerfishController __instance)
        {
            anglerfish.Add(__instance);
            updateAnglerfish(__instance);
            return true;
        }

        private static bool AnglerfishControllerOnDestroy(ref AnglerfishController __instance)
        {
            anglerfish.Remove(__instance);
            createdAnglerfish.Remove(__instance);
            return true;
        }

        private static bool AnglerfishControllerOnFeel(ref ImpactData impact)
        {
            if (!canFeel)
            {
                var attachedOwRigidbody = impact.otherCollider.GetAttachedOWRigidbody();
                if ((attachedOwRigidbody.CompareTag("Player") || attachedOwRigidbody.CompareTag("Ship")))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool AnglerfishControllerOnHearSound(ref NoiseMaker noiseMaker)
        {
            return canHear;
        }

        private static bool AnglerfishControllerMoveTowardsTarget(ref AnglerfishController __instance, ref Vector3 targetPos, ref float moveSpeed, ref float maxAcceleration)
        {
            Vector3 vector3 = targetPos - (__instance.GetValue<OWRigidbody>("_anglerBody").GetPosition() + __instance.transform.TransformDirection(__instance.GetValue<Vector3>("_mouthOffset")));
            Vector3 relativeVelocity = __instance.GetValue<OWRigidbody>("_brambleBody").GetRelativeVelocity(__instance.GetValue<OWRigidbody>("_anglerBody"));
            Vector3 acceleration = vector3.normalized * moveSpeed - relativeVelocity;

            // Make them not fly past the player
            var proposedAcceleration = Mathf.Min(acceleration.magnitude, maxAcceleration);
            var target = __instance.GetValue<OWRigidbody>("_targetBody");
            if (target)
            {
                var destinationVelocity = __instance.GetValue<OWRigidbody>("_brambleBody").GetRelativeVelocity(target).magnitude + (moveSpeed / 5f);
                var timeToDestination = (2f * (vector3.magnitude + 100f)) / (destinationVelocity + relativeVelocity.magnitude);
                var requiredDecceleration = (destinationVelocity - relativeVelocity.magnitude) / (timeToDestination * timeToDestination);

                if (proposedAcceleration < Mathf.Abs(requiredDecceleration) && Mathf.Abs(requiredDecceleration) < maxAcceleration)
                {
                    proposedAcceleration = requiredDecceleration;
                }
                if (maxAcceleration < Mathf.Abs(requiredDecceleration))
                {
                    maxAcceleration = -1f * maxAcceleration;
                }
            }

            __instance.GetValue<OWRigidbody>("_anglerBody").AddAcceleration(acceleration.normalized * proposedAcceleration);
            return false;
        }

        public static void createAnglerfish(AnglerfishController anglerfishController)
        {
            if (anglerfishController.enabled && createdAnglerfish.Count == 0)
            {
                var parent = Locator.GetAstroObject(AstroObject.Name.TimberHearth)?.GetAttachedOWRigidbody();
                if (parent)
                {
                    var controller = AnglerfishController.Instantiate(anglerfishController, parent.GetPosition() + new Vector3(0f, 300f, 0f), Quaternion.identity, parent.transform);
                    controller.GetAttachedOWRigidbody().SetVelocity(parent.GetVelocity());
                    controller.SetValue("_brambleBody", parent);
                    controller.SetSector(SectorHelper.getSector(Sector.Name.Sun)[0].GetRootSector());
                    createdAnglerfish.Add(controller);
                }
            }
        }

        private static void hardUpdateAnglerfish()
        {
            if (Time.time - _lastHardUpdate < 1f)
            {
                _lastHardUpdate = Time.time;
                foreach (AnglerfishController anglerfishController in createdAnglerfish)
                {
                    SetVisible(anglerfishController, true, true);
                }
                foreach (AnglerfishController anglerfishController in anglerfish)
                {
                    if (anglerfishController.isActiveAndEnabled)
                    {
                        var currentPosition = anglerfishController.GetAttachedOWRigidbody().GetPosition();
                        if ((currentPosition - anglerfishController.GetValue<OWRigidbody>("_brambleBody").GetPosition()).magnitude > 1000f)
                        {
                            var nearestBody = Position.getClosest(currentPosition);
                            var body = Position.getBody(nearestBody);
                            if ((body.GetPosition() - currentPosition).magnitude > 1000f)
                            {
                                body = Position.getBody(Position.HeavenlyBodies.Sun);
                            }
                            anglerfishController.SetValue("_brambleBody", body);
                            anglerfishController.transform.parent = body.transform;
                        }
                    }
                }
            }
        }

        private static void updateAnglerfish()
        {
            foreach (AnglerfishController anglerfishController in anglerfish)
            {
                updateAnglerfish(anglerfishController);
            }
        }

        private static void updateAnglerfish(AnglerfishController anglerfishController)
        {
            if (enabledAI != null)
                anglerfishController.enabled = enabledAI.Value;
            updateParameter(anglerfishController, "_acceleration", _overrideAcceleration);
            updateParameter(anglerfishController, "_investigateSpeed", _overrideInvestigateSpeed);
            updateParameter(anglerfishController, "_chaseSpeed", _overrideChaseSpeed);
            updateParameter(anglerfishController, "_turnSpeed", _overrideTurnSpeed);
            updateParameter(anglerfishController, "_quickTurnSpeed", _overrideQuickTurnSpeed);
            updateParameter(anglerfishController, "_arrivalDistance", _overrideMouthOpenDistance);
            updateParameter(anglerfishController, "_pursueDistance", _overridePursueDistance);
            updateParameter(anglerfishController, "_escapeDistance", _overrideEscapeDistance);

            if (anglerfishController.isActiveAndEnabled)
            {
                var currentPosition = anglerfishController?.GetAttachedOWRigidbody()?.GetPosition();
                var parentPosition = anglerfishController?.GetValue<OWRigidbody>("_brambleBody")?.GetPosition();
                if (currentPosition.HasValue && parentPosition.HasValue && (currentPosition.Value - parentPosition.Value).magnitude > 1000f)
                {
                    var nearestBody = Position.getClosest(currentPosition.Value);
                    var body = Position.getBody(nearestBody);
                    if (body && (body.GetPosition() - currentPosition.Value).magnitude > 1000f)
                    {
                        body = Position.getBody(Position.HeavenlyBodies.Sun);
                    }

                    if (body)
                    {
                        anglerfishController.SetValue("_brambleBody", body);
                        anglerfishController.transform.parent = body.transform;
                    }
                }
            }

            if (!anglerfishSkin)
            {
                var skin = anglerfishController?.GetComponentInChildren<SkinnedMeshRenderer>()?.sharedMesh;
                if (skin)
                {
                    anglerfishSkin = GameObject.Instantiate(skin);
                }
            }

            if (!canStun)
            {
                updateParameter(anglerfishController, "_stunTimer", 0f);
            }

            if (anglerfishController.isActiveAndEnabled)
            {
                if (canSmell && getPlayerBody() && anglerfishController.transform && (anglerfishController.GetAnglerState() == AnglerfishController.AnglerState.Investigating || anglerfishController.GetAnglerState() == AnglerfishController.AnglerState.Lurking))
                {
                    var distance = getPlayerBody().GetPosition() - anglerfishController.transform.position;
                    var bramble = getParameter<OWRigidbody>(anglerfishController, "_brambleBody");

                    if (distance.magnitude < _smellDistance && bramble?.transform)
                    {
                        anglerfishController.SetValue("_localDisturbancePos", bramble.transform.InverseTransformPoint(getPlayerBody().GetPosition()));
                        anglerfishController.Invoke("ChangeState", AnglerfishController.AnglerState.Investigating);
                    }
                }
                if (canSee && getPlayerBody() && anglerfishController.transform && (anglerfishController.GetAnglerState() == AnglerfishController.AnglerState.Investigating || anglerfishController.GetAnglerState() == AnglerfishController.AnglerState.Lurking))
                {
                    var distance = getPlayerBody().GetPosition() - anglerfishController.transform.position;
                    var xAngle = Vector3.Angle(distance, anglerfishController.transform.forward);
                    var yAngle = Vector3.Angle(distance, anglerfishController.transform.up) - _visionYoffset;

                    if (distance.magnitude <= _visionDistance && (xAngle * 2) <= _visionX && 0 <= yAngle && yAngle <= _visionY)
                    {
                        anglerfishController.SetValue("_targetBody", getPlayerBody());
                        anglerfishController.Invoke("ChangeState", AnglerfishController.AnglerState.Chasing);
                    }
                }
            }
        }

        private static OWRigidbody getPlayerBody()
        {
            if (PlayerState.IsInsideShip())
            {
                return Locator.GetShipBody();
            }
            else
            {
                return Locator.GetPlayerBody();
            }
        }

        private static void updateParameter(string id, float? parameter, float defaultValue)
        {
            foreach (AnglerfishController anglerfishController in anglerfish)
            {
                anglerfishController.SetValue(id, parameter.GetValueOrDefault(defaultValue));
            }
        }

        private static void updateParameter(AnglerfishController anglerfishController, string id, float? parameter)
        {
            if (parameter != null)
            {
                anglerfishController.SetValue(id, parameter.Value);
            }
        }

        private static T getParameter<T>(string id, T defaultValue)
        {
            foreach (AnglerfishController anglerfishController in anglerfish)
            {
                return getParameter<T>(anglerfishController, id);
            }
            return defaultValue;
        }

        private static T getParameter<T>(AnglerfishController anglerfishController, string id)
        {
            return anglerfishController.GetValue<T>(id);
        }

        private static void SetVisible(AnglerfishController item, bool visible, bool collision)
        {
            // TODO: figure out why the quailty reduction
            // TODO: Get Rendered version at game start
            var skin = item.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skin)
            {
                if (visible && !skin.sharedMesh)
                {
                    skin.sharedMesh = anglerfishSkin;
                }
                else if (!visible && skin.sharedMesh)
                {
                    skin.sharedMesh = null;
                }
            }

            SetVisibleBehaviour(item, visible);
            foreach (OWCollider collider in item.GetComponentsInChildren<OWCollider>())
            {
                SetVisibleBehaviour(collider, visible);
                collider.SetActivation(true);
                collider.SetLODLevel(collision ? 0 : 1, 0f);
                if (collider.GetCollider())
                {
                    collider.GetCollider().enabled = collision;
                    SetVisibleComponent(collider, visible);
                }
            }
            foreach (Collider collider in item.GetComponentsInChildren<Collider>())
            {
                collider.enabled = collision;
                SetVisibleComponent(collider, visible);
            }
            foreach (Renderer render in item.GetComponentsInChildren<Renderer>())
            {
                render.enabled = true;
                SetVisibleComponent(render, visible);
            }
            foreach (Light light in item.GetComponentsInChildren<Light>())
            {
                SetVisibleBehaviour(light, visible);
            }
            foreach (LightLOD light in item.GetComponentsInChildren<LightLOD>())
            {
                SetVisibleBehaviour(light, visible);
            }
            foreach (FogLight light in item.GetComponentsInChildren<FogLight>())
            {
                SetVisibleBehaviour(light, visible);
            }
        }


        private static void SetVisibleBehaviour(Behaviour item, bool visible)
        {
            item.enabled = true;
            SetVisibleComponent(item, visible);
        }

        private static void SetVisibleComponent(Component item, bool visible)
        {
            item?.gameObject?.SetActive(true);
            if (item.GetAttachedOWRigidbody())
            {
                item.GetAttachedOWRigidbody().enabled = true;
            }

            if (visible)
            {
                item?.GetAttachedOWRigidbody()?.Unsuspend();
            }
            else
            {
                item?.GetAttachedOWRigidbody()?.Suspend();
            }
        }
    }
}

using OWML.Utils;
using PacificEngine.OW_CommonResources.Game.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Game.Component.Detector
{
    public static class GravityDetector
    {
        public static T[] getForceDetectors<T>(GameObject body) where T: ForceDetector
        {
            return body.GetComponentsInChildren<T>();
        }

        public static GameObject registerConstantForceDetector(OWRigidbody body, HeavenlyBody[] boundBodies, ForceDetector inheritDetector = null, bool inherit = true)
        {
            if (boundBodies != null)
            {
                GameObject detector = new GameObject("FieldDetector");
                detector.SetActive(false);
                detector.transform.parent = body.transform;
                detector.transform.localPosition = Vector3.zero;
                detector.layer = 20;

                ConstantForceDetector forceDetector = detector.AddComponent<ConstantForceDetector>();
                if (inheritDetector != null)
                {
                    forceDetector.SetValue("_inheritDetector", inheritDetector);
                    forceDetector.SetValue("_activeInheritedDetector", inheritDetector);
                }
                forceDetector.SetValue("_inheritElement0", inherit);

                var volumes = new List<ForceVolume>();
                foreach (HeavenlyBody parent in boundBodies)
                {
                    var parentBody = Position.getBody(parent);
                    var volume = parentBody?.GetAttachedOWRigidbody()?.GetAttachedGravityVolume();
                    if (volume != null)
                    {
                        volumes.Add(volume);
                    }
                }
                forceDetector.SetValue("_detectableFields", volumes.ToArray());
                body.RegisterAttachedForceDetector(forceDetector);

                detector.SetActive(true);
                return detector;
            }

            return null;
        }
    }
}

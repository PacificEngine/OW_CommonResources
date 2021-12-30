using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Game.State
{
    public static class GameTimer
    {
        private static long totalGUI;
        private static long totalUpdates;
        private static long totalLateUpdates;
        private static long totalFixedUpdates;
        private static long lastAwakeFrames;
        private static long lastAwakeUpdates;
        private static long lastSceneLoadFrames;
        private static long lastSceneLoadUpdates;

        public static long FramesSinceStart { get { return totalFixedUpdates; } }
        public static long FramesSinceAwake { get { return totalFixedUpdates - lastAwakeFrames; } }
        public static long FramesSinceSceneLoad { get { return totalFixedUpdates - lastSceneLoadFrames; } }
        public static long CyclesSinceStart { get { return totalUpdates; } }
        public static long CyclesSinceAwake { get { return totalUpdates - lastAwakeUpdates; } }
        public static long CyclesSinceSceneLoad { get { return totalUpdates - lastSceneLoadUpdates; } }

        public static void Start()
        {
            totalGUI = 0;
            totalUpdates = 0;
            totalLateUpdates = 0;
            totalFixedUpdates = 0;
        }

        public static void Awake()
        {
            lastAwakeFrames = totalFixedUpdates;
            lastAwakeUpdates = totalFixedUpdates;
        }

        public static void Destroy()
        {
        }

        public static void SceneLoaded()
        {
            lastSceneLoadFrames = totalFixedUpdates;
            lastSceneLoadUpdates = totalFixedUpdates;
        }

        public static void OnGUI()
        {
            totalGUI++;
        }

        public static void Update()
        {
            totalUpdates++;
        }

        public static void LateUpdate()
        {
            totalLateUpdates++;
        }

        public static void FixedUpdate()
        {
            totalFixedUpdates++;
        }
    }
}

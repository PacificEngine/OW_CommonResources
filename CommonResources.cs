using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game;
using PacificEngine.OW_CommonResources.Game.Display;
using PacificEngine.OW_CommonResources.Game.Player;
using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Game.State;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PacificEngine.OW_CommonResources
{
    public class MainClass : ModBehaviour
    {
        void Start()
        {
            Helper.helper = ModHelper;
            ModHelper.Events.Player.OnPlayerAwake += onAwake;
            SceneManager.sceneLoaded += onSceneLoaded;

            GameTimer.Start();
            Position.Start();
            Fog.Start();
            Anglerfish.Start();
            Inhabitants.Start();
            Items.Start();
            EyeCoordinates.Start();
            BramblePortals.Start();
            WarpPad.Start();
            Data.Start();
            Planet.Start();
            Tracker.Start();

            ModHelper.Console.WriteLine("Common Resources Mod: Started!");
        }

        void Destory()
        {
            Helper.helper = ModHelper;
            ModHelper.Events.Player.OnPlayerAwake -= onAwake;
            SceneManager.sceneLoaded -= onSceneLoaded;

            GameTimer.Destroy();
            Position.Destroy();
            Fog.Destroy();
            Anglerfish.Destroy();
            Inhabitants.Destroy();
            Items.Destroy();
            EyeCoordinates.Destroy();
            BramblePortals.Destroy();
            WarpPad.Destroy();
            Planet.Destroy();
            Tracker.Destroy();

            ModHelper.Console.WriteLine("Common Resources Mod: Clean Up!");
        }

        void onSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Helper.helper = ModHelper;
            GameTimer.SceneLoaded();
            Planet.SceneLoaded();
        }

        void onAwake(PlayerBody player)
        {
            Helper.helper = ModHelper;
            GameTimer.Awake();
            Position.Awake();
            Fog.Awake();
            Anglerfish.Awake();
            Inhabitants.Awake();
            Items.Awake();
            EyeCoordinates.Awake();
            BramblePortals.Awake();
            WarpPad.Awake();
            Planet.Awake();
            Tracker.Awake();
            ModHelper.Console.WriteLine("Common Resources Mod: Player Awakes");
        }

        public override void Configure(IModConfig config)
        {
            Helper.helper = ModHelper;
        }

        void OnGUI()
        {
            Helper.helper = ModHelper;
            GameTimer.OnGUI();
            DisplayConsole.OnGUI();
        }

        void Update()
        {
            Helper.helper = ModHelper;
            GameTimer.Update();
            Position.Update();
            Fog.Update();
            Player.Update();
            Ship.Update();
            Anglerfish.Update();
            SuperNova.Update();
            EyeCoordinates.Update();
            BramblePortals.Update();
            WarpPad.Update();
            Planet.Update();
            Tracker.Update();
        }

        void LateUpdate()
        {
            Helper.helper = ModHelper;
            GameTimer.LateUpdate();
        }

        void FixedUpdate()
        {
            Helper.helper = ModHelper;
            GameTimer.FixedUpdate();
            Position.FixedUpdate();
            Fog.FixedUpdate();
            Player.FixedUpdate();
            Ship.FixedUpdate();
            Anglerfish.FixedUpdate();
            SuperNova.FixedUpdate();
            EyeCoordinates.FixedUpdate();
            BramblePortals.FixedUpdate();
            WarpPad.FixedUpdate();
            Planet.FixedUpdate();
            Tracker.FixedUpdate();
        }
    }
}

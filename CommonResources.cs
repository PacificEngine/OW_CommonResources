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

namespace PacificEngine.OW_CommonResources
{
    public class MainClass : ModBehaviour
    {
        void Start()
        {
            Helper.helper = (ModHelper)ModHelper;
            ModHelper.Events.Player.OnPlayerAwake += (player) => onAwake();
            Position.Start();
            Fog.Start();
            Anglerfish.Start();
            Inhabitants.Start();
            Items.Start();
            EyeCoordinates.Start();
            BramblePortals.Start();
            WarpPad.Start();
            Data.Start();
            ModHelper.Console.WriteLine("Common Resources Mod: Started!");
        }

        void Destory()
        {
            Helper.helper = (ModHelper)ModHelper;
            Position.Destroy();
            Fog.Destroy();
            Anglerfish.Destroy();
            Inhabitants.Destroy();
            Items.Destroy();
            EyeCoordinates.Destroy();
            BramblePortals.Destroy();
            WarpPad.Destroy();
            ModHelper.Console.WriteLine("Common Resources Mod: Clean Up!");
        }

        void onAwake()
        {
            Helper.helper = (ModHelper)ModHelper;
            Position.Awake();
            Fog.Awake();
            Anglerfish.Awake();
            Inhabitants.Awake();
            EyeCoordinates.Awake();
            BramblePortals.Awake();
            WarpPad.Awake();
            ModHelper.Console.WriteLine("Common Resources Mod: Player Awakes");
        }

        public override void Configure(IModConfig config)
        {
            Helper.helper = (ModHelper)ModHelper;
        }

        void OnGUI()
        {
            DisplayConsole.OnGUI();
        }

        void Update()
        {
            Helper.helper = (ModHelper)ModHelper;
            Position.Update();
            Fog.Update();
            Player.Update();
            Ship.Update();
            Anglerfish.Update();
            SuperNova.Update();
            EyeCoordinates.Update();
            BramblePortals.Update();
            WarpPad.Update();
        }
    }
}

using PacificEngine.OW_CommonResources.Game.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Game.Player
{
    public static class Possession
    {
        public static void Start()
        {
        }

        public static void Destroy()
        {
        }

        public static ItemTool getItemTool()
        {
            return UnityEngine.GameObject.FindObjectOfType<ItemTool>();
        }

        public static void pickUpWarpCore(WarpCoreType type)
        {
            var tool = getItemTool();
            if (tool)
            {
                tool.PickUpItemInstantly(Items.createWarpCore(type));
            }
        }
    }
}

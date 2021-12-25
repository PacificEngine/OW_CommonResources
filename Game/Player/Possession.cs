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

        public static void pickUpSlideReel(Items.SlideReelStory story, bool burned)
        {
            pickUpItem(Items.createSlideReel(story, burned));
        }

        public static void pickUpVisionTorch()
        {
            pickUpItem(Items.createVisionTorch());
        }

        public static void pickUpShareStone(NomaiRemoteCameraPlatform.ID type)
        {
            pickUpItem(Items.createShareStone(type));
        }

        public static void pickUpConversationStone(NomaiWord type)
        {
            pickUpItem(Items.createConversationStone(type));
        }

        public static void pickUpLantern(bool broken, bool lit)
        {
            pickUpItem(Items.createLantern(broken, lit));
        }

        public static void pickUpDreamLantern(DreamLanternType type, bool lit)
        {
            pickUpItem(Items.createDreamLantern(type, lit));
        }

        public static void pickUpWarpCore(WarpCoreType type)
        {
            pickUpItem(Items.createWarpCore(type));
        }

        public static void pickUpItem(OWItem item)
        {
            var tool = getItemTool();
            if (tool != null && item != null && item.gameObject != null)
            {
                if (tool.GetHeldItem() == null)
                {
                    tool.PickUpItemInstantly(item);
                }
            }
        }
    }
}

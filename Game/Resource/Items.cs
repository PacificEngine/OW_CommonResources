using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PacificEngine.OW_CommonResources.Game.Resource
{
    public static class Items
    {
        private static HashSet<SlideCollectionContainer> inverted = new HashSet<SlideCollectionContainer>();

        public enum SlideReelStory
        {
            None,
            Story_1,
            Story_2,
            Story_3,
            Story_4,
            Story_5_Complete,
            Story_5_NoVessel,
            Story_5_NoInterloper,
            Story_5_NoInterloperNoVessel,
            LibraryPath_1,
            LibraryPath_2,
            LibraryPath_3,
            Seal_1,
            Seal_2,
            Seal_3,
            DreamRule_1,
            DreamRule_2_v1,
            DreamRule_2_v2,
            DreamRule_3,
            Burning,
            Experiment,
            DamageReport,
            LanternSecret,
            SupernovaEscape,
            SignalJammer,
            Homeworld,
            PrisonPeephole_Vision,
            PrisonerFarewellVision,
            TowerVision
        }

        public static void Start()
        {
            Helper.helper.HarmonyHelper.AddPrefix<SlideCollectionContainer>("OnTextureLoaded", typeof(Items), "onPrefixSlideCollectionContainerOnTextureLoaded");
        }

        public static void Awake()
        {
            inverted.Clear();
        }

        public static void Destroy()
        {
        }

        public static ScrollItem createScroll()
        {
            // TODO
            return null;
        }

        public static SlideReelItem createSlideReel(SlideReelStory type, bool burned)
        {

            var burnS = burned ? "Burned" : "Complete";
            String name;
            String container;
            bool invert = false;
            switch (type)
            {
                case SlideReelStory.Story_1:
                    name = $"Reel_1_Story_{burnS}";
                    container = name;
                    break;
                case SlideReelStory.Story_2:
                    name = $"Reel_2_Story_{burnS}";
                    container = name;
                    break;
                case SlideReelStory.Story_3:
                    name = $"Reel_3_Story_{burnS}";
                    container = name;
                    break;
                case SlideReelStory.Story_4:
                    name = $"Reel_4_Story_Burned";
                    container = name;
                    if (!burned)
                    {
                        container = "Reel_4_Story_Vision";
                        invert = true;
                    }
                    break;
                case SlideReelStory.LibraryPath_1:
                    name = $"Reel_1_LibraryPath";
                    container = name;
                    break;
                case SlideReelStory.LibraryPath_2:
                    name = $"Reel_2_LibraryPath";
                    container = name;
                    break;
                case SlideReelStory.LibraryPath_3:
                    name = $"Reel_3_LibraryPath";
                    container = name;
                    break;
                case SlideReelStory.Seal_1:
                    name = $"Reel_1_Seal";
                    container = name;
                    break;
                case SlideReelStory.Seal_2:
                    name = $"Reel_2_Seal";
                    container = name;
                    break;
                case SlideReelStory.Seal_3:
                    name = $"Reel_3_Seal";
                    container = name;
                    break;
                case SlideReelStory.DreamRule_1:
                    name = $"Reel_1_DreamRule";
                    container = name;
                    break;
                case SlideReelStory.DreamRule_2_v1:
                    name = $"Reel_2_DreamRule_v1";
                    container = name;
                    break;
                case SlideReelStory.DreamRule_2_v2:
                    name = $"Reel_2_DreamRule_v2";
                    container = name;
                    break;
                case SlideReelStory.DreamRule_3:
                    name = $"Reel_3_DreamRule";
                    container = name;
                    break;
                case SlideReelStory.Burning:
                    name = $"Reel_Burning";
                    container = name;
                    break;
                case SlideReelStory.Experiment:
                    if (burned)
                    {
                        name = $"Reel_ExperimentWatch_{burnS}";
                    }
                    else
                    {
                        name = $"Reel_ExperimentWatch";
                    }
                    container = name;
                    break;
                case SlideReelStory.DamageReport:
                    name = $"Reel_DamageReport";
                    container = name;
                    break;
                case SlideReelStory.LanternSecret:
                    name = $"Reel_LanternSecret";
                    container = name;
                    break;
                case SlideReelStory.Story_5_Complete:
                    name = "Reel_Destroyed_8";
                    container = "Reel_5_Story_Vision_Complete";
                    invert = true;
                    break;
                case SlideReelStory.Story_5_NoVessel:
                    name = "Reel_Destroyed_8";
                    container = "Reel_5_Story_Vision_NoVessel";
                    invert = true;
                    break;
                case SlideReelStory.Story_5_NoInterloper:
                    name = "Reel_Destroyed_8";
                    container = "Reel_5_Story_Vision_NoInterloper";
                    invert = true;
                    break;
                case SlideReelStory.Story_5_NoInterloperNoVessel:
                    name = "Reel_Destroyed_8";
                    container = "Reel_5_Story_Vision_NoInterloperNoVessel";
                    invert = true;
                    break;
                case SlideReelStory.PrisonPeephole_Vision:
                    name = "Reel_Destroyed_8";
                    container = "Reel_PrisonPeephole_Vision";
                    invert = true;
                    break;
                case SlideReelStory.PrisonerFarewellVision:
                    name = "Reel_Destroyed_8";
                    container = "Reel_PrisonerFarewellVision";
                    invert = true;
                    break;
                case SlideReelStory.TowerVision:
                    name = "Reel_Destroyed_8";
                    container = "Reel_TowerVision";
                    invert = true;
                    break;
                case SlideReelStory.SignalJammer:
                    name = "Reel_Destroyed_8";
                    container = "AutoProjector_SignalJammer";
                    break;
                case SlideReelStory.Homeworld:
                    name = "Reel_Destroyed_8";
                    container = "AutoProjector_Homeworld";
                    break;
                case SlideReelStory.SupernovaEscape:
                    name = "Reel_Destroyed_8";
                    container = "AutoProjector_SupernovaEscape";
                    break;
                default:
                    name = "Reel_Destroyed_8";
                    container = name;
                    break;
            }
            foreach (SlideReelItem reel in Resources.FindObjectsOfTypeAll<SlideReelItem>())
            {
                if (reel.name.Contains(name))
                {
                    var newReel = GameObject.Instantiate(reel, (Locator.GetAstroObject(AstroObject.Name.Sun)?.transform ?? Locator.GetAstroObject(AstroObject.Name.Eye)?.transform ?? Locator.GetPlayerBody()?.transform));
                    SetVisible(newReel, true);
                    if (invert)
                    {
                        inverted.Add(newReel.slidesContainer);
                    }
                    if (!name.Equals(container))
                    {
                        newReel.slidesContainer.ClearSlides();
                        newReel.slidesContainer.SetValue("_changeSlidesAllowed", true);
                        HashSet<int> expendedIndex = new HashSet<int>();
                        foreach (SlideCollectionContainer collection in Resources.FindObjectsOfTypeAll<SlideCollectionContainer>())
                        {
                            if (collection.name.Contains(container))
                            {
                                newReel.slidesContainer.SetValue("_shipLogOnComplete", collection.GetValue<string>("_shipLogOnComplete"));
                                newReel.slidesContainer.SetValue("_autoLoadStreaming", collection.GetValue<bool>("_autoLoadStreaming"));
                                newReel.slidesContainer.SetValue("_invertBlackFrames", collection.GetValue<bool>("_invertBlackFrames"));
                                newReel.slidesContainer.SetValue("_musicRanges", collection.GetValue<List<SlideCollectionContainer.SlideMusicRange>>("_musicRanges"));
                                newReel.slidesContainer.slideCollection.streamingAssetIdentifier = collection.slideCollection.streamingAssetIdentifier;
                                newReel.slidesContainer.slideCollection.SetAssetBundle(collection.slideCollection.GetAssetBundle());
                                foreach (var slide in collection.slideCollection.slides)
                                {
                                    if (!expendedIndex.Contains(slide.GetStreamingIndex()))
                                    {
                                        expendedIndex.Add(slide.GetStreamingIndex());
                                        var newSlide = new Slide(slide);
                                        newReel.slidesContainer.AddSlide(newSlide);
                                    }
                                }
                            }
                        }
                    }
                    return newReel;
                }
            }
            return null;
        }

        private static bool onPrefixSlideCollectionContainerOnTextureLoaded(ref SlideCollectionContainer __instance, ref int index, ref Texture texture)
        {
            if (inverted.Contains(__instance))
            {
                foreach (var slide in __instance.slideCollection.slides)
                {
                    if (slide.GetStreamingIndex() == index)
                    {
                        slide._image = invertColors(makeReadable(texture)) as Texture2D;
                    }
                }

                if (index == __instance.GetCurrentSlide().GetStreamingIndex())
                {
                    __instance.GetCurrentSlide().InvokeTextureUpdate();
                }

                return false;
            }

            return true;
        }

        private static Texture makeReadable(Texture texture)
        {
            if (!texture.isReadable)
            {
                RenderTexture renderTex = RenderTexture.GetTemporary(
                            texture.width,
                            texture.height,
                            0,
                            RenderTextureFormat.Default,
                            RenderTextureReadWrite.Linear);

                Graphics.Blit(texture, renderTex);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = renderTex;
                Texture2D readableText = new Texture2D(texture.width, texture.height);
                readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                readableText.Apply();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(renderTex);

                return readableText;
            }

            return texture;
        }

        private static Texture invertColors(Texture texture)
        {
            Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
            var colors = (texture as Texture2D).GetPixels();
            var newColors = new Color[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                newColors[i] = new Color(1f - color.r, 1f - color.g, 1f - color.b, color.a);
            }
            newTexture.SetPixels(newColors);
            newTexture.Apply();

            return newTexture;
        }

        public static SharedStone createShareStone(NomaiRemoteCameraPlatform.ID type)
        {
            foreach (SharedStone stone in Resources.FindObjectsOfTypeAll<SharedStone>())
            {
                if (stone.GetRemoteCameraID().Equals(type))
                {
                    var newStone = GameObject.Instantiate(stone, (Locator.GetAstroObject(AstroObject.Name.Sun)?.transform ?? Locator.GetAstroObject(AstroObject.Name.Eye)?.transform ?? Locator.GetPlayerBody()?.transform));
                    SetVisible(newStone, true);
                    return newStone;
                }
            }
            return null;
        }

        public static VisionTorchItem createVisionTorch()
        {
            foreach (VisionTorchItem torch in Resources.FindObjectsOfTypeAll<VisionTorchItem>())
            {
                var newTorch = GameObject.Instantiate(torch, (Locator.GetAstroObject(AstroObject.Name.Sun)?.transform ?? Locator.GetAstroObject(AstroObject.Name.Eye)?.transform ?? Locator.GetPlayerBody()?.transform));
                SetVisible(newTorch, true);
                return newTorch;
            }
            return null;
        }

        public static NomaiConversationStone createConversationStone(NomaiWord type)
        {
            foreach (NomaiConversationStone stone in Resources.FindObjectsOfTypeAll<NomaiConversationStone>())
            {
                if (stone.GetWord().Equals(type))
                {
                    var newStone = GameObject.Instantiate(stone, (Locator.GetAstroObject(AstroObject.Name.Sun)?.transform ?? Locator.GetAstroObject(AstroObject.Name.Eye)?.transform ?? Locator.GetPlayerBody()?.transform));
                    SetVisible(newStone, true);
                    return newStone;
                }
            }
            return null;
        }

        public static SimpleLanternItem createLantern(bool broken, bool lit)
        {
            foreach (SimpleLanternItem lantern in Resources.FindObjectsOfTypeAll<SimpleLanternItem>())
            {
                if (lantern.IsInteractable()
                    && (broken == lantern.name.Contains("BROKEN")))
                {
                    var newLantern = GameObject.Instantiate(lantern, (Locator.GetAstroObject(AstroObject.Name.Sun)?.transform ?? Locator.GetAstroObject(AstroObject.Name.Eye)?.transform ?? Locator.GetPlayerBody()?.transform));
                    SetVisible(newLantern, true);
                    lantern.SetValue("_startsLit", lit);
                    lantern.SetValue("_lit", lit);
                    newLantern.SetValue("_lightSourceShape", lantern.GetValue<SphereShape>("_lightSourceShape"));
                    return newLantern;
                }
            }
            return null;
        }

        public static DreamLanternItem createDreamLantern(DreamLanternType type, bool lit)
        {
            foreach (DreamLanternItem lantern in Resources.FindObjectsOfTypeAll<DreamLanternItem>())
            {
                if (lantern.IsInteractable()
                    && lantern.GetLanternType().Equals(type))
                {
                    var newLantern = GameObject.Instantiate(lantern, (Locator.GetAstroObject(AstroObject.Name.Sun)?.transform ?? Locator.GetAstroObject(AstroObject.Name.Eye)?.transform ?? Locator.GetPlayerBody()?.transform));
                    SetVisible(newLantern, true);
                    if (newLantern.GetLanternController())
                    {
                        newLantern.SetLit(true);
                    }
                    return newLantern;
                }
            }
            return null;
        }

        public static WarpCoreItem createWarpCore(WarpCoreType type)
        {
            foreach (WarpCoreItem core in Resources.FindObjectsOfTypeAll<WarpCoreItem>())
            {
                if (core.GetWarpCoreType().Equals(type))
                {
                    var newCore = GameObject.Instantiate(core, (Locator.GetAstroObject(AstroObject.Name.Sun)?.transform ?? Locator.GetAstroObject(AstroObject.Name.Eye)?.transform ?? Locator.GetPlayerBody()?.transform));
                    SetVisible(newCore, true);
                    return newCore;
                }
            }
            return null;
        }

        private static void SetVisible(OWItem item, bool visible)
        {
            item.SetValue("_visible", visible);
            foreach (OWRenderer render in item.GetValue<OWRenderer[]>("_renderers"))
            {
                if (render)
                {
                    render.enabled = true;
                    render.SetActivation(true);
                    render.SetLODActivation(visible);
                    if (render.GetRenderer())
                    {
                        render.GetRenderer().enabled = true;
                    }
                }
            }
            foreach (ParticleSystem particleSystem in item.GetValue<ParticleSystem[]>("_particleSystems"))
            {
                if (particleSystem)
                {
                    if (visible)
                        particleSystem.Play(true);
                    else
                        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
            foreach (OWLight2 light in item.GetValue<OWLight2[]>("_lights"))
            {
                if (light)
                {
                    light.enabled = true;
                    light.SetActivation(true);
                    light.SetLODActivation(visible);
                    if (light.GetLight())
                    {
                        light.GetLight().enabled = true;
                    }
                }
            }
        }
    }
}

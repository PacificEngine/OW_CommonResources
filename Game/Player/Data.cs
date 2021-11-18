using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PacificEngine.OW_CommonResources.Game.Player
{
    public static class Data
    {
        public static bool launchCodes
        {
            get
            { 
                return PlayerData.IsLoaded() && PlayerData.KnowsLaunchCodes();
            }
            set
            {
                if (PlayerData.IsLoaded() && value != launchCodes)
                {
                    if (value)
                    {
                        PlayerData.LearnLaunchCodes();
                        DialogueConditionManager.SharedInstance.SetConditionState("TalkedToHornfels", true);
                        DialogueConditionManager.SharedInstance.SetConditionState("SCIENTIST_3", true);
                        DialogueConditionManager.SharedInstance.SetConditionState("LAUNCH_CODES_GIVEN", true);
                        StandaloneProfileManager.SharedInstance.currentProfileGameSave.SetPersistentCondition("LAUNCH_CODES_GIVEN", true);
                        GlobalMessenger.FireEvent(nameof(PlayerData.LearnLaunchCodes));
                        GameObject.FindWithTag("Global")?.GetComponent<KeyInfoPromptController>()?.Invoke("OnLearnLaunchCodes");
                    }
                    else
                    {
                        DialogueConditionManager.SharedInstance.SetConditionState("TalkedToHornfels", false);
                        DialogueConditionManager.SharedInstance.SetConditionState("SCIENTIST_3", false);
                        DialogueConditionManager.SharedInstance.SetConditionState("LAUNCH_CODES_GIVEN", false);
                        StandaloneProfileManager.SharedInstance.currentProfileGameSave.SetPersistentCondition("LAUNCH_CODES_GIVEN", false);
                        GameObject.FindWithTag("Global")?.GetComponent<KeyInfoPromptController>()?.Invoke("OnLaunchCodesEntered");
                    }
                    PlayerData.SaveCurrentGame();
                }
            }
        }

        public static bool eyeCoordinates
        {
            get
            {
                return PlayerData.IsLoaded() && Locator.GetShipLogManager() && Locator.GetShipLogManager().IsFactRevealed("OPC_EYE_COORDINATES_X1");
            }
            set
            {
                if (PlayerData.IsLoaded() && Locator.GetShipLogManager() && value != eyeCoordinates)
                {
                    learnFacts(value, false, "OPC_EYE_COORDINATES_X1");
                    GameObject.FindWithTag("Global")?.GetComponent<KeyInfoPromptController>()?.GetValue<ScreenPrompt>("_eyeCoordinatesPrompt")?.SetVisibility(value);
                }
            }
        }

        public static bool knowAllSignals
        {
            get
            {
                if (!PlayerData.IsLoaded())
                {
                    return false;
                }

                foreach (SignalName signal in (SignalName[])Enum.GetValues(typeof(SignalName)))
                {
                    if (signal != SignalName.Default && !PlayerData.KnowsSignal(signal))
                    {
                        return false;
                    }
                }
                return true;
            }
            set
            {
                if (!PlayerData.IsLoaded())
                {
                    return;
                }
                learnSignal(value, (SignalName[])Enum.GetValues(typeof(SignalName)));
            }
        }

        public static bool knowAllFrequencies
        {
            get
            {
                if (!PlayerData.IsLoaded())
                {
                    return false;
                }

                foreach (SignalFrequency frequency in (SignalFrequency[])Enum.GetValues(typeof(SignalFrequency)))
                {
                    if (frequency != SignalFrequency.Default && !PlayerData.KnowsFrequency(frequency))
                    {
                        return false;
                    }
                }
                return true;
            }
            set
            {
                if (!PlayerData.IsLoaded())
                {
                    return;
                }

                if (value)
                {
                    foreach (SignalFrequency frequency in (SignalFrequency[])Enum.GetValues(typeof(SignalFrequency)))
                    {
                        PlayerData.LearnFrequency(frequency);
                    }
                }
                else
                {
                    PlayerData.ForgetFrequency(SignalFrequency.Quantum);
                    PlayerData.ForgetFrequency(SignalFrequency.EscapePod);
                    PlayerData.ForgetFrequency(SignalFrequency.Statue);
                    PlayerData.ForgetFrequency(SignalFrequency.WarpCore);
                    PlayerData.ForgetFrequency(SignalFrequency.HideAndSeek);
                    PlayerData.ForgetFrequency(SignalFrequency.Radio);
                }
                PlayerData.SaveCurrentGame();
            }
        }

        public static bool knowAllRumors
        {
            get
            {
                if (!Locator.GetShipLogManager())
                {
                    return false;
                }

                return Locator.GetShipLogManager().GetValue<List<ShipLogFact>>("_factList").TrueForAll(x => !x.IsRumor() || x.IsRevealed());
            }
            set
            {
                if (!Locator.GetShipLogManager())
                {
                    return;
                }
                learnFacts(value, false, Locator.GetShipLogManager().GetValue<List<ShipLogFact>>("_factList").FindAll(x => x.IsRumor()).ToArray());
            }
        }

        public static bool knowAllFacts
        {
            get
            {
                if (!Locator.GetShipLogManager())
                {
                    return false;
                }

                return Locator.GetShipLogManager().GetValue<List<ShipLogFact>>("_factList").TrueForAll(x => x.IsRumor() || x.IsRevealed());
            }
            set
            {
                if (!Locator.GetShipLogManager())
                {
                    return;
                }
                learnFacts(value, false, Locator.GetShipLogManager().GetValue<List<ShipLogFact>>("_factList").FindAll(x => value || !x.IsRumor()).ToArray());
            }
        }

        public static void learnSignal(bool learn = true, params SignalName[] signals)
        {
            if (StandaloneProfileManager.SharedInstance.currentProfileGameSave != null)
            {
                foreach (var signal in signals)
                {
                    if (learn)
                    {
                        StandaloneProfileManager.SharedInstance.currentProfileGameSave.knownSignals[(int)signal] = true;
                    }
                    else
                    {
                        StandaloneProfileManager.SharedInstance.currentProfileGameSave.knownSignals.Remove((int)signal);
                    }
                }
                PlayerData.SaveCurrentGame();
            }
        }

        public static void learnFacts(bool learn = true, bool notifyPlayer = false, params ShipLogFact[] facts)
        {
            learnFacts(learn, notifyPlayer, new List<ShipLogFact>(facts).ConvertAll(x => x.GetID() ?? x.GetEntryID()).ToArray());
        }

        public static void learnFacts(bool learn = true, bool notifyPlayer = false, params String[] factIds)
        {
            if (!Locator.GetShipLogManager())
            {
                return;
            }

            foreach (var fact in factIds)
            {
                if (learn)
                {
                    Locator.GetShipLogManager().RevealFact(fact, false, false);
                }
                else
                {
                    var savedFact = StandaloneProfileManager.SharedInstance.currentProfileGameSave.shipLogFactSaves[fact];
                    savedFact.newlyRevealed = false;
                    savedFact.read = false;
                    savedFact.revealOrder = -1;
                }
            }
            PlayerData.SaveCurrentGame();
        }

        public static EntryData? getFactEntry(string factId)
        {
            if (!Locator.GetShipLogManager())
            {
                return null;
            }

            var library = Locator.GetShipLogManager().GetValue<ShipLogLibrary>("_shipLogLibrary");
            if (library != null && library.entryData != null)
            {
                for (int i = 0; i < library.entryData.Length; i++)
                {
                    var libraryEntry = library.entryData[i];
                    if (factId.Equals(libraryEntry.id))
                    {
                        return libraryEntry;
                    }
                }
            }

            return null;
        }

        public static void setFactCardImage(string factId, Sprite sprite, Sprite altSprite)
        {
            if (!Locator.GetShipLogManager())
            {
                return;
            }

            var library = Locator.GetShipLogManager()?.GetValue<ShipLogLibrary>("_shipLogLibrary");
            if (library != null && library.entryData != null)
            {
                for (int i = 0; i < library.entryData.Length; i++)
                {
                    var libraryEntry = library.entryData[i];
                    if (factId.Equals(libraryEntry.id))
                    {
                        libraryEntry.sprite = sprite;
                        libraryEntry.altSprite = altSprite;
                        library.entryData[i] = libraryEntry;
                    }
                }
            }

            var dictionary = Locator.GetShipLogManager()?.GetValue<Dictionary<string, EntryData>>("_entryDataDict");
            if (dictionary != null)
            {
                EntryData dictionaryEntry;
                if (dictionary.TryGetValue(factId, out dictionaryEntry))
                {
                    dictionaryEntry.sprite = sprite;
                    dictionaryEntry.altSprite = altSprite;
                    dictionary[factId] = dictionaryEntry;
                }
            }

            var entryList = Locator.GetShipLogManager()?.GetEntryList();
            if (entryList != null)
            {
                foreach (var shipEntry in Locator.GetShipLogManager().GetEntryList())
                {
                    if (factId.Equals(shipEntry.GetID()))
                    {
                        shipEntry.SetSprite(sprite);
                        shipEntry.SetAltSprite(altSprite);
                    }
                }
            }
        }
    }
}

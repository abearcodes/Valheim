using System;
using System.Collections.Generic;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Patches;
using BepInEx.Configuration;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers
{
    public class PluginSettings
    {
        private ConfigEntry<string> _allowedContainerLookupPieceNames;

        public PluginSettings(ConfigFile configFile)
        {
            BindConfig(configFile);
        }

        public ConfigEntry<bool> CraftingWithContainersEnabled { get; private set; }

        public ConfigEntry<float> ContainerLookupRange { get; private set; }

        public ConfigEntry<bool> TakeFromPlayerInventoryFirst { get; private set; }

        public ConfigEntry<string> AllowedContainerLookupPieceNames
        {
            get => _allowedContainerLookupPieceNames;
            set
            {
                void SplitNewValueAndSetProperty()
                {
                    AllowedContainerLookupPieceNamesAsList = value.Value.Split(',')
                        .Select(entry => entry.Trim())
                        .ToList();
                }

                _allowedContainerLookupPieceNames = value;
                value.SettingChanged += (sender, args) => { SplitNewValueAndSetProperty(); };
                SplitNewValueAndSetProperty();
            }
        }

        public List<string> AllowedContainerLookupPieceNamesAsList { get; private set; }

        public ConfigEntry<bool> ShouldFilterByContainerPieceNames { get; private set; }

        public ConfigEntry<bool> DebugViableContainerIndicatorEnabled { get; private set; }

        public ConfigEntry<bool> AllowTakeFuelForKilnAndFurnace { get; private set; }

        public ConfigEntry<string> LastPluginVersionUsed { get; private set; }

        public ConfigEntry<bool> AllowTakeFuelForFireplace { get; private set; }
        public ConfigEntry<bool> LogItemRemovalsToConsole { get; private set; }
        public ConfigEntry<bool> AddExtractionEffectWhenCrafting { get; private set; }
        public ConfigEntry<bool> DebugForcePrintRemovalReport { get; set; }

        private void BindConfig(ConfigFile configFile)
        {
            // General
            CraftingWithContainersEnabled = configFile.Bind("General",
                "Enabled", true,
                "Enable using resources from nearby containers.\n" +
                "Enables/disables the main functionality of the mod");
            TakeFromPlayerInventoryFirst = configFile.Bind("General",
                "TakeFromPlayerInventoryFirst", false,
                "Prioritize taking items from the players inventory when crafting");
            ContainerLookupRange = configFile.Bind("General",
                "ContainerLookupRange", 20.0f,
                "Range in which the mod searches for containers.\n" +
                "Base range is equal to the range of the crafting table (20).\n" +
                "Will not take from containers that are not currently loaded into memory,\n" +
                "so setting this value to big numbers might not yield the expected result.");
            LogItemRemovalsToConsole = configFile.Bind("General",
                "LogItemRemovalsToConsole", true,
                "If enabled, item removal requests will be logged to the ingame\n" +
                "console (accessible via F5)");
            AddExtractionEffectWhenCrafting = configFile.Bind("General",
                "AddExtractionEffectWhenCrafting", true,
                "If enabled, when removing items from containers an effect just like the one\n" +
                "that crafting station extensions (chopping block, tanning rack, etc)");
            // ShowStationExtensionEffect = configFile.Bind("CraftingWithContainers",
            //     "ShowStationExtensionEffect", true,
            //     "Adds a station extension effect to chests. This effect is the one that\n" +
            //     "the game uses by default for chopping blocks, tanning decks, etc\n" +
            //     "Shouldn't influence performance");

            AllowTakeFuelForKilnAndFurnace = configFile.Bind("Interactions",
                "AllowTakeFuelForKilnAndFurnace", true,
                "If true, will allow the mod to take fuel from nearby containers when using\n" +
                "Kilns and Furnaces.\n");
            
            AllowTakeFuelForFireplace = configFile.Bind("Interactions",
                "AllowTakeFuelForFireplace", true,
                "If true, will allow the mod to take fuel from nearby containers when using\n" +
                "Fireplaces and Hearths\n");


            // Filter
            ShouldFilterByContainerPieceNames = configFile.Bind("Filtering",
                "ShouldFilterByContainerPieceNames", false,
                "If enabled, will filter the linked containers by it's owning object name.\n" +
                "For example, you might want to not link carts or ships.");
            AllowedContainerLookupPieceNames = configFile.Bind("Filtering",
                "AllowedContainerLookupPieceNames",
                string.Join(", ", "$piece_chestwood", "$piece_chest", "$piece_chestprivate", "Cart", "$ship_karve",
                    "$ship_longship"),
                "Comma separated list of filtered \"holders\" for the containers:" +
                "chests, carts, ships. Uses the name of the \"Piece\" the container is attached to");

            // Versioning
            // todo: implement actual logic for this. for now we want to just keep track  
            LastPluginVersionUsed = configFile.Bind("Versioning",
                "LastPluginVersionUsed", "1.0.4",
                "CraftingWithContainers version marker. Used to notify the user about updates");
            
            // Debug
            DebugViableContainerIndicatorEnabled = configFile.Bind("zDebug",
                "DebugViableContainerIndicatorEnabled", false,
                "Shows nearby viable containers by adding a small indicator on containers that are\n" +
                "considered viable according to the current settings.");
            DebugForcePrintRemovalReport = configFile.Bind("zDebug",
                "DebugAlwaysPrintRemovalReport", false,
                "If enabled, prints the removal reports always and not only when issues happen");
        }

        private void NotifyNeedsRestart(object sender, EventArgs e)
        {
            const string text = "You have changed a setting that requires a game restart. " +
                                "The changes won't be active until you do so. ";
            const string craftingWithContainersPrefix = "CraftingWithContainers";
            Plugin.Log.LogWarning(text);
            Console.instance.Print($"{craftingWithContainersPrefix}: {text}");
            Chat.instance.AddString(craftingWithContainersPrefix, text, Talker.Type.Shout);
            AccessTools.Field(typeof(Chat), "m_hideTimer").SetValue(Chat.instance, -10f);
        }
    }
}
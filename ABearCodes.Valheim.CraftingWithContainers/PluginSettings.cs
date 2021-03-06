﻿using System;
using System.Collections.Generic;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Common;
using BepInEx.Configuration;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers
{
    public class PluginSettings
    {
        private ConfigEntry<string> _allowedContainerLookupPieceNames;
        private ConfigEntry<string> _allowedKilnFuels;
        private ConfigEntry<string> _allowedSmelterOres;

        public PluginSettings(ConfigFile configFile)
        {
            BindConfig(configFile);
        }

        public ConfigEntry<bool> CraftingWithContainersEnabled { get; private set; }

        public ConfigEntry<float> ContainerLookupRange { get; private set; }

        public ConfigEntry<string> AllowedContainerLookupPieceNames
        {
            get => _allowedContainerLookupPieceNames;
            set
            {
                void SplitNewValueAndSetProperty()
                {
                    AllowedContainerLookupPieceNamesAsList = value.Value.Split(',')
                        .Select(entry => entry.Trim())
                        .Where(entry => !string.IsNullOrEmpty(entry))
                        .ToList();
                }

                _allowedContainerLookupPieceNames = value;
                value.SettingChanged += (sender, args) => { SplitNewValueAndSetProperty(); };
                SplitNewValueAndSetProperty();
            }
        }

        private ConfigEntry<string> AllowedKilnFuels
        {
            get => _allowedKilnFuels;
            set
            {
                void SplitNewValueAndSetProperty()
                {
                    AllowedKilnFuelsAsList = value.Value.Split(',')
                        .Select(entry => entry.Trim())
                        .Where(entry => !string.IsNullOrEmpty(entry))
                        .ToList();
                }

                _allowedKilnFuels = value;
                value.SettingChanged += (sender, args) => { SplitNewValueAndSetProperty(); };
                SplitNewValueAndSetProperty();
            }
        }
        public List<string> AllowedKilnFuelsAsList { get; set; }
        private ConfigEntry<string> AllowedSmelterOres
        {
            get => _allowedSmelterOres;
            set
            {
                void SplitNewValueAndSetProperty()
                {
                    AllowedSmelterOresAsList = value.Value.Split(',')
                        .Select(entry => entry.Trim())
                        .Where(entry => !string.IsNullOrEmpty(entry))
                        .ToList();
                }

                _allowedSmelterOres = value;
                value.SettingChanged += (sender, args) => { SplitNewValueAndSetProperty(); };
                SplitNewValueAndSetProperty();
            }
        }


        public List<string> AllowedSmelterOresAsList { get; set; }


        public List<string> AllowedContainerLookupPieceNamesAsList { get; private set; }

        public ConfigEntry<bool> ShouldFilterByContainerPieceNames { get; private set; }

        public ConfigEntry<bool> DebugViableContainerIndicatorEnabled { get; private set; }

        public ConfigEntry<bool> AllowTakeFuelForKilnAndFurnace { get; private set; }

        public ConfigEntry<string> LastPluginVersionUsed { get; private set; }

        public ConfigEntry<bool> AllowTakeFuelForFireplace { get; private set; }
        public ConfigEntry<bool> LogItemRemovalsToConsole { get; private set; }
        public ConfigEntry<bool> AddExtractionEffectWhenCrafting { get; private set; }
        public ConfigEntry<bool> DebugForcePrintRemovalReport { get; set; }
        public ConfigEntry<bool> TakeItemsInReverseOrder { get; set; }
        public ConfigEntry<bool> ModifyItemCountIndicator { get; set; }
        
        public ConfigEntry<bool> DebugViableContainerIndicatorDetailedEnabled { get; set; }

        private void BindConfig(ConfigFile configFile)
        {
            // General
            CraftingWithContainersEnabled = configFile.Bind("General",
                "Enabled", true,
                "Enable using resources from nearby containers.\n" +
                "Enables/disables the main functionality of the mod");
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
            TakeItemsInReverseOrder = configFile.Bind("General",
                "TakeItemsInReverseOrder", true,
                "If enabled, will take items from the inventories from the last slot first\n" +
                "instead of the first slot how the game does for normal item removals");
            // ShowStationExtensionEffect = configFile.Bind("CraftingWithContainers",
            //     "ShowStationExtensionEffect", true,
            //     "Adds a station extension effect to chests. This effect is the one that\n" +
            //     "the game uses by default for chopping blocks, tanning decks, etc\n" +
            //     "Shouldn't influence performance");

            // Smelting & else
            AllowTakeFuelForKilnAndFurnace = configFile.Bind("Interactions",
                "AllowTakeFuelForKilnAndFurnace", true,
                "If true, will allow the mod to take fuel from nearby containers when using\n" +
                "Kilns and Furnaces.\n");

            AllowTakeFuelForFireplace = configFile.Bind("Interactions",
                "AllowTakeFuelForFireplace", true,
                "If true, will allow the mod to take fuel from nearby containers when using\n" +
                "Fireplaces and Hearths\n");

            AllowedKilnFuels = configFile.Bind("Interactions",
                "AllowedKilnFuels", "$item_wood",
                "List of allowed fuels to be used by Kilns. If empty, will use any fuel.\n" +
                "By default, only allows normal wood.\n" +
                "Resources available as of 0.147.3 (use the left hand side identifier):\n" +
                "$item_wood - Normal Wood\n" +
                "$item_finewood - Fine Wood\n" +
                "$item_roundlog - Core Wood\n");

            AllowedSmelterOres = configFile.Bind("Interactions",
                "AllowedSmelterOres", "",
                "List of allowed ores to be pulled into smelters. If empty, will use any ore.\n" +
                "By default, allows all ores.\n" +
                "Resources available as of 0.147.3 (use the left hand side identifier):\n" +
                "$item_copperore - CopperOre\n" +
                "$item_ironore - IronOre\n" +
                "$item_ironscrap - IronScrap\n" +
                "$item_tinore - TinOre\n" +
                "$item_silverore - SilverOre\n");            

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

            // UI
            ModifyItemCountIndicator = configFile.Bind("UI", "PatchRequirementsUI", true,
                "If enabled, will add an indicator to the amount of required resources\n" +
                "within the crafting UI, showing the total amount of items available (with containers)\n" +
                "You might want to disable this if you are having issues with other mods that also touch\n" +
                "modify the requirements UI.\n" +
                "Enabled by default. ");


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
            DebugViableContainerIndicatorDetailedEnabled = configFile.Bind("zDebug",
                "DebugViableContainerDetailedEnabled", false,
                "Instead of using a \"small\" indicator for viable chests, displays the chests\n" +
                "inventory hashcode + owner ZDO userid.");
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
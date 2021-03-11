using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace ABearCodes.Valheim.SimpleRecycling
{
    public class PluginSettings
    {
        private ConfigEntry<string> _stationFilterListString;

        public List<string> StationFilterList = new List<string>();

        public PluginSettings(ConfigFile config)
        {
            RecyclingRate = config.Bind("General", "RecyclingRate", 0.5f,
                "Rate at which the resources are recycled. Value must be between 0 and 1.\n" +
                "The mod always rolls *down*, so if you were supposed to get 2.5 items, you would only receive 2.");
            RecyclingRate.SettingChanged += (sender, args) =>
            {
                if (RecyclingRate.Value > 1.0f) RecyclingRate.Value = 1.0f;
                if (RecyclingRate.Value < 0f) RecyclingRate.Value = 0f;
            };
            UnstackableItemsAlwaysReturnAtLeastOneResource = config.Bind("General",
                "UnstackableItemsAlwaysReturnAtLeastOneResource", true,
                "If enabled and recycling a specific _unstackable_ item would yield 0 of a material,\n" +
                "instead you will receive 1. If disabled, you get nothing.");

            PreventZeroResourceYields = config.Bind("General", "PreventZeroResourceYields", true,
                "If enabled and recycling an item would yield 0 of any material,\n" +
                "instead you will receive 1. If disabled, you get nothing.");

            AllowRecyclingUnknownRecipes = config.Bind("General", "AllowRecyclingUnknownRecipes", false,
                "If enabled, it will allow you to recycle items that you do not know the recipe for yet.\n" +
                "Disabled by default as this can be cheaty, but sometimes required due to people losing progress.");

            ContainerRecyclingButtonPositionJsonString = config.Bind("Recycling on containers",
                "ContainerButtonPosition",
                "{\"x\":502.42425537109377,\"y\":147.06060791015626,\"z\":-1.0}",
                "The last saved recycling button position stored in JSON");

            // UI
            ContainerRecyclingEnabled = config.Bind("UI", "ContainerRecyclingEnabled",
                false, "If enabled, the mod will display the container recycling button");

            NotifyOnSalvagingImpediments = config.Bind("UI", "NotifyOnSalvagingImpediments", true,
                "If enabled and recycling a specific item runs into any issues, the mod will print a message\n" +
                "in the center of the screen (native Valheim notification). At the time of implementation,\n" +
                "this happens in the following cases:\n" +
                " - not enough free slots in the inventory to place the resulting resources\n" +
                " - player does not know the recipe for the item\n" +
                " - if enabled, cases when `PreventZeroResourceYields` kicks in and prevent the crafting");

            EnableExperimentalCraftingTabUI = config.Bind("UI", "EnableExperimentalCraftingTabUI", true,
                "If enabled, will display the experimental work in progress crafting tab UI\n" +
                "Enabled by default.");

            HideEquippedItemsInRecyclingTab = config.Bind("UI", "HideRecipesForEquippedItems", true,
                "If enabled, it will hide equipped items in the crafting tab.\n" +
                "This does not make the item recyclable and only influences whether or not it's shown.\n" +
                "Enabled by default.");

            IgnoreItemsOnHotbar = config.Bind("UI", "IgnoreItemsOnHotbar", true,
                "If enabled, it will hide hotbar items in the crafting tab.\n" +
                "Enabled by default.");
            
            StationFilterEnabled = config.Bind("UI", "StationFilterEnabled", true,
                "If enabled, will filter all recycling recipes based on the crafting station\n" +
                "used to produce said item. Main purpose of this is to prevent showing food\n" +
                "as a recyclable item, but can be extended further if needed.\n" +
                "Enabled by default");

            StationFilterListString = config.Bind("UI", "StationFilterList", "$piece_cauldron",
                "Comma separated list of crafting stations (by their \"piece name\")\n" +
                "recipes from which should be ignored in regards to recycling.\n" +
                "Main purpose of this is to prevent showing food as a recyclable item,\n" +
                "but can be extended further if needed.\n" +
                "\n" +
                "Full list of stations used in recipes as of 0.147.3:\n" +
                "- identifier: `$piece_forge` in game name: Forge\n" +
                "- identifier: `$piece_workbench` in game name: Workbench\n" +
                "- identifier: `$piece_cauldron` in game name: Cauldron\n" +
                "- identifier: `$piece_stonecutter` in game name: Stonecutter\n" +
                "\n" +
                "Use the identifiers, not the in game names (duh!)");

            // debug
            DebugAlwaysDumpAnalysisContext = config.Bind("zDebug", "DebugAlwaysDumpAnalysisContext", false,
                "If enabled will dump a complete detailed recycling report every time. This is taxing in terms\n" +
                "of performance and should only be used when debugging issues. ");
            DebugAllowSpammyLogs = config.Bind("zDebug", "DebugAllowSpammyLogs", false,
                "If enabled, will spam recycling checks to the console.\n" +
                "VERY. VERY. SPAMMY. Influences performance. ");

            NexusID = config.Bind("zUtil", "NexusID", 205, "Nexus mod ID for updates");
        }

        private ConfigEntry<string> StationFilterListString
        {
            get => _stationFilterListString;
            set
            {
                void SplitNewValueAndSetProperty()
                {
                    StationFilterList = value.Value.Split(',')
                        .Select(entry => entry.Trim())
                        .ToList();
                }

                _stationFilterListString = value;
                value.SettingChanged += (sender, args) => { SplitNewValueAndSetProperty(); };
                SplitNewValueAndSetProperty();
            }
        }

        public ConfigEntry<bool> StationFilterEnabled { get; }

        public ConfigEntry<bool> EnableExperimentalCraftingTabUI { get; }

        public ConfigEntry<int> NexusID { get; set; }

        public ConfigEntry<bool> NotifyOnSalvagingImpediments { get; }

        public ConfigEntry<bool> PreventZeroResourceYields { get; }

        public ConfigEntry<bool> UnstackableItemsAlwaysReturnAtLeastOneResource { get; }

        public ConfigEntry<float> RecyclingRate { get; }

        public ConfigEntry<bool> ContainerRecyclingEnabled { get; }
        public ConfigEntry<bool> IgnoreItemsOnHotbar { get; }
        public ConfigEntry<string> ContainerRecyclingButtonPositionJsonString { get; }
        public ConfigEntry<bool> AllowRecyclingUnknownRecipes { get; }
        public ConfigEntry<bool> DebugAlwaysDumpAnalysisContext { get; }
        public ConfigEntry<bool> DebugAllowSpammyLogs { get; }
        public ConfigEntry<bool> HideEquippedItemsInRecyclingTab { get; }
    }
}
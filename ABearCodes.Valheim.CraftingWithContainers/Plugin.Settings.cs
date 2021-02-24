using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace ABearCodes.Valheim.CraftingWithContainers
{
    public class PluginSettings
    {
        private ConfigEntry<string> _allowedContainerLookupPieceNames;

        public PluginSettings(ConfigFile configFile)
        {
            BindConfig(configFile);
        }

        public ConfigEntry<bool> CraftingWithContainersEnabled { get; set; }

        public ConfigEntry<float> ContainerLookupRange { get; set; }

        public ConfigEntry<bool> TakeFromPlayerInventoryFirst { get; set; }

        public ConfigEntry<bool> ShowStationExtensionEffect { get; set; }

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

        public ConfigEntry<bool> ShouldFilterByContainerPieceNames { get; set; }

        public ConfigEntry<bool> DebugViableContainerIndicatorEnabled { get; set; }

        private void BindConfig(ConfigFile configFile)
        {
            CraftingWithContainersEnabled = configFile.Bind("CraftingWithContainers",
                "Enabled", true,
                "Enable using resources from nearby containers.\n" +
                "Enables/disables the main functionality of the mod");
            TakeFromPlayerInventoryFirst = configFile.Bind("CraftingWithContainers",
                "TakeFromPlayerInventoryFirst", false,
                "Prioritize taking items from the players inventory when crafting");
            ContainerLookupRange = configFile.Bind("CraftingWithContainers",
                "ContainerLookupRange", 10.0f,
                "Multiplier for the range in which the mod searches for containers.\n" +
                "Base range is equal to the range of the crafting table in use.\n" +
                "Will not take from containers that are not currently loaded into memory.");
            ShowStationExtensionEffect = configFile.Bind("CraftingWithContainers",
                "ShowStationExtensionEffect", true,
                "Adds a station extension effect to chests. This effect is the one that\n" +
                "the game uses by default for chopping blocks, tanning decks, etc\n" +
                "Shouldn't influence performance");
            ShouldFilterByContainerPieceNames = configFile.Bind("CraftingWithContainers.Filtering",
                "ShouldFilterByContainerPieceNames", false,
                "If enabled, will filter the linked containers by it's owning object name.\n" +
                "For example, you might want to not link carts or ships.");
            AllowedContainerLookupPieceNames = configFile.Bind("CraftingWithContainers.Filtering",
                "AllowedContainerLookupPieceNames",
                string.Join(", ", "$piece_chestwood", "$piece_chest", "$piece_chestprivate", "Cart", "$ship_karve",
                    "$ship_longship"),
                "Comma separated list of filtered \"holders\" for the containers:" +
                "chests, carts, ships. Uses the name of the \"Piece\" the container is attached to");
            DebugViableContainerIndicatorEnabled = configFile.Bind("CraftingWithContainers",
                "DebugViableContainerIndicatorEnabled", true,
                "");
        }
    }
}
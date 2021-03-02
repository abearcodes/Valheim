using BepInEx.Configuration;

namespace ABearCodes.Valheim.SimpleRecycling
{
    public class PluginSettings
    {
        public PluginSettings(ConfigFile config)
        {
            RecyclingRate = config.Bind("General", "RecyclingRate", 0.5f,
                "Rate at which the resources are recycled. Value must be between 0 and 1.\n" +
                "The mod always rolls up, so if you were supposed to get 2.5 items, you would only receive 2.");
            UnstackableItemsAlwaysReturnAtLeastOneResource = config.Bind("General", "UnstackableItemsAlwaysReturnAtLeastOneResource", true,
                "If enabled and recycling a specific _unstackable_ item would yield 0 of a material,\n" +
                "instead you will receive 1. If disabled, you get nothing.");
            
            ContainerRecyclingEnabled = config.Bind("Recycling on containers", "ContainerRecyclingEnabled",
                true, "If enabled, the mod will display the container recycling button");
            ContainerRecyclingButtonPositionJsonString = config.Bind("Recycling on containers",
                "ContainerButtonPosition",
                "{\"x\":502.42425537109377,\"y\":147.06060791015626,\"z\":-1.0}",
                "The last saved recycling button position stored in JSON");
        }

        public ConfigEntry<bool> OnlyContainersNextToChoppingBlocks { get; }

        public ConfigEntry<bool> UnstackableItemsAlwaysReturnAtLeastOneResource { get;  }


        public ConfigEntry<float> RecyclingRate { get; }

        public ConfigEntry<bool> ContainerRecyclingEnabled { get; }

        public ConfigEntry<string> ContainerRecyclingButtonPositionJsonString { get; }
    }
}
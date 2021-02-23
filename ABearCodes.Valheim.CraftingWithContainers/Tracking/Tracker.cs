using System.Collections.Generic;

namespace ABearCodes.Valheim.CraftingWithContainers.Tracking
{
    public static class Tracker
    {
        public static List<ContainerEntry> AllContainers = new List<ContainerEntry>();

        public static List<CraftingLock> CraftingPlayers = new List<CraftingLock>();

        public static CraftingLock FindLockByPlayer(Inventory inventory)
        {
            return CraftingPlayers.Find(entry => entry.Player.GetInventory() == inventory);
        }
        public class ContainerEntry
        {
            public ContainerEntry(Container container,
                ContainerCraftingNetworkExtension containerCraftingNetworkExtension)
            {
                Container = container;
                ContainerCraftingNetworkExtension = containerCraftingNetworkExtension;
            }

            public Container Container { get; }
            public ContainerCraftingNetworkExtension ContainerCraftingNetworkExtension { get; }
        }

        public class CraftingLock
        {
            public CraftingLock(Player player, int expectedEntries)
            {
                Player = player;
                ExpectedEntries = expectedEntries;
            }

            public Player Player { get; }
            public List<ContainerEntry> Entries { get; } = new List<ContainerEntry>();
            
            public int ExpectedEntries { get; }
            public int DeniedEntries { get; set; }
        }
    }
}
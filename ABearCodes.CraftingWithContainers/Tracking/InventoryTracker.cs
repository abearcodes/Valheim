using System.Collections.Generic;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Tracking
{
    public static class InventoryTracker
    {
        public static Dictionary<long, LinkedInventories> ExpandedPlayerInventories =
            new Dictionary<long, LinkedInventories>();


        public struct LinkedInventories
        {
            public LinkedInventories(Player player, CraftingStation station, IEnumerable<Container> containers,
                List<GameObject> effects)
            {
                Player = player;
                Station = station;
                Containers = containers;
                Effects = effects;
            }

            public Player Player { get; }
            public CraftingStation Station { get; }

            public List<GameObject> Effects { get; }
            public IEnumerable<Container> Containers { get; }
        }
    }
}
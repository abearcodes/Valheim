using System;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Crafting
{
    public static class CraftingHelper
    {
        // ReSharper disable once UnusedMember.Global
        public static int CountItemsArea(this Player player, string itemName)
        {
            var playerItemCount = player.GetInventory().CountItems(itemName);
            var containers = ContainerTracker
                .GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            var containerItemCount = containers
                .Sum(container => container.Container.GetInventory().CountItems(itemName));
            // Plugin.Log.LogDebug(
            //     $"Player ${player.GetPlayerID()} found {playerItemCount}+{containerItemCount} of {itemName} via {containers.Count}");
            return playerItemCount + containerItemCount;
        }
    }
}
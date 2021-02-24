using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Crafting
{
    // public static class CraftingHandler
    // {
    //     public static int CountItemsArea(this Player player, string itemName)
    //     {
    //         Debug.Log($"Player ${player?.GetPlayerID()} - {itemName}");
    //         var pid = player.GetPlayerID();
    //         return ContainerTrackingExtensions
    //             .FindContainersInRange(pid, player.transform.position, Plugin.Settings.ContainerLookupRange.Value)
    //             .Sum(container => container.GetInventory().CountItems(itemName));
    //     }
    // }
}
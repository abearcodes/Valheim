using System;
using System.Collections.Generic;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using HarmonyLib;
using UnityEngine;
using ABearCodes.Valheim.CraftingWithContainers.Patches;
using ABearCodes.Valheim.CraftingWithContainers.Utils;
using JetBrains.Annotations;

namespace ABearCodes.Valheim.CraftingWithContainers.Crafting
{
    public static class CraftingHelper
    {
        [UsedImplicitly]
        public static int CountItemsArea(this Inventory inventory, string itemName)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value ||
                !ContainerTracker.PlayerByInventoryDict.TryGetValue(inventory.GetHashCode(), out var player)) 
                return inventory.CountItems(itemName);
            // only run our handler if we are tracking the inventory, hence it's a 
            // player inventory. otherwise - let the default execute as is
            
            var playerItemCount = inventory.CountItems(itemName);
            var containers = ContainerTracker
                .GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            var containerItemCount = containers
                .Sum(container => container.Container.GetInventory().CountItems(itemName));
            return playerItemCount + containerItemCount;
        }
        
        [UsedImplicitly]
        public static bool HaveItemArea(this Inventory inventory, string itemName)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value ||
                !ContainerTracker.PlayerByInventoryDict.TryGetValue(inventory.GetHashCode(), out var player)) 
                return inventory.HaveItem(itemName);
            
            var playerHasAny = player.GetInventory().HaveItem(itemName);
            var containers = ContainerTracker
                .GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            var containersHaveAny = containers
                .Any(container => container.Container.GetInventory().HaveItem(itemName));
            Plugin.Log.LogDebug(
                $"Player ${player.GetPlayerID()} found {playerHasAny}+{containersHaveAny} of {itemName} via {containers.Count}");
            return playerHasAny || containersHaveAny;
        }

        [UsedImplicitly]
        public static void RemoveItemArea(this Inventory inventory, string name, int amount)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value ||
                !ContainerTracker.PlayerByInventoryDict.TryGetValue(inventory.GetHashCode(), out var player))
            {
                inventory.RemoveItem(name, amount);
                return;
            }
            
            var containers = ContainerTracker.GetViableContainersInRangeForPlayer(player,
                Plugin.Settings.ContainerLookupRange.Value);
            IterateAndRemoveItemsFromInventories(player, containers, name, amount);
        }
        
        private static void IterateAndRemoveItemsFromInventories(Player player, List<TrackedContainer> containers,
            string name, int amount)
        {
            var leftToRemove = amount;
            if (Plugin.Settings.TakeFromPlayerInventoryFirst.Value)
            {
                leftToRemove = player.GetInventory().RemoveItemAsMuchAsPossible(name, amount);
            }
            foreach (var container in containers)
            {
                leftToRemove = container.Container.GetInventory().RemoveItemAsMuchAsPossible(name, leftToRemove);
                UpdateContainerNetworkData(player, container.Container);
                if (leftToRemove == 0)
                    break;
            }
            if (!Plugin.Settings.TakeFromPlayerInventoryFirst.Value && leftToRemove > 0)
            {
                player.GetInventory().RemoveItemAsMuchAsPossible(name, amount);
            }
        }

        private static void UpdateContainerNetworkData(Player player, Container container)
        {
            var containerZNewView = (ZNetView) AccessTools.Field(typeof(Container), "m_nview").GetValue(container);
            container.Save();
            var containerUid = containerZNewView.GetZDO().m_uid;
            ZDOMan.instance.ForceSendZDO(player.GetPlayerID(), containerUid);
            containerZNewView.GetZDO().SetOwner(player.GetPlayerID());
        }


        
    }
}
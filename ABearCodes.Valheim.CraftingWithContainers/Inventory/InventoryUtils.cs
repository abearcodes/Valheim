using System.Collections.Generic;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Common;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Inventory
{
    public static class InventoryUtils
    {
        /// <summary>
        ///     Removes as much as possible from an inventory and returns the amount left to remove
        /// </summary>
        /// <returns>amount of items taken</returns>
        public static int RemoveItemAsMuchAsPossible(this global::Inventory inventory, string name, int requestedAmount)
        {
            var currentInventoryCount = inventory.CountItemsOriginal(name);
            var itemsToTake = currentInventoryCount < requestedAmount
                ? currentInventoryCount
                : requestedAmount;
            if (Plugin.Settings.TakeItemsInReverseOrder.Value)
                inventory.RemoveItemReversed(name, requestedAmount);
            else
                inventory.RemoveItemOriginal(name, itemsToTake);
            Plugin.Log.LogDebug($"Removed {itemsToTake} from inv {inventory.GetHashCode()}");
            return itemsToTake;
        }

        public static void RemoveItemReversed(this global::Inventory inventory, string name, int amount)
        {
            var m_inventory =
                (List<ItemDrop.ItemData>) AccessTools.Field(typeof(global::Inventory), "m_inventory").GetValue(inventory);
            for (var index = m_inventory.Count - 1; index >= 0; index--)
            {
                var itemData = m_inventory[index];
                if (itemData.m_shared.m_name == name)
                {
                    var num = Mathf.Min(itemData.m_stack, amount);
                    itemData.m_stack -= num;
                    amount -= num;
                    if (amount <= 0)
                        break;
                }
            }

            m_inventory.RemoveAll(x => x.m_stack <= 0);
            inventory.Changed();
        }
        
    }
}
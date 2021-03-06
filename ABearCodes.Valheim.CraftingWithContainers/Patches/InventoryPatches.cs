using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Patches
{
    [HarmonyPatch]
    public class InventoryPatches
    {
        // todo: evaluate if there's a prettier way, this is a complete copy-paste detour
        [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(string), typeof(int))]
        [HarmonyPrefix]
        public static bool RemoveItemReversed(Inventory __instance, string name, int amount, List<ItemDrop.ItemData> ___m_inventory)
        {
            if (!Plugin.Settings.TakeItemsInReverseOrder.Value) return true;

            for (var index = ___m_inventory.Count - 1 ; index >= 0; index--)
            {
                var itemData = ___m_inventory[index];
                if (itemData.m_shared.m_name == name)
                {
                    var num = Mathf.Min(itemData.m_stack, amount);
                    itemData.m_stack -= num;
                    amount -= num;
                    if (amount <= 0)
                        break;
                }
            }

            ___m_inventory.RemoveAll(x => x.m_stack <= 0);
            __instance.Changed();
            return false;
        }
        [HarmonyPatch(typeof(Inventory), "GetItem", typeof(string))]
        [HarmonyPrefix]
        public static bool GetItem(Inventory __instance, string name, List<ItemDrop.ItemData> ___m_inventory, ItemDrop.ItemData __result)
        {
            if (!Plugin.Settings.TakeItemsInReverseOrder.Value) return true;

            for (var index = ___m_inventory.Count - 1; index >= 0; index--)
            {
                ItemDrop.ItemData itemData = ___m_inventory[index];
                if (itemData.m_shared.m_name != name) continue;
                __result = itemData;
                return false;
            }
            __result = null;
            return false;
        }
    }
}
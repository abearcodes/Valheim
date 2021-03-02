using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ABearCodes.Valheim.SimpleRecycling.Recycling
{
    public static class Recycler
    {
        public static void RecycleInventoryForAllRecipes(Inventory inventory, List<Recipe> recipes, Player player)
        {
            var itemListSnapshot = new List<ItemDrop.ItemData>();
            // copy the inventory, otherwise collection will constantly change causing issues
            itemListSnapshot.AddRange(inventory.GetAllItems());
            for (var index = 0; index < itemListSnapshot.Count; index++)
            {
                var item = itemListSnapshot[index];
                var recipe = recipes
                    .FirstOrDefault(r => r.m_item.m_itemData.m_shared.m_name == item.m_shared.m_name);
                if (recipe == null) continue;

                //todo: optimize two .GetComponent<ItemDrop> calls 
                var recyclingList = GatherRecyclingResultForItem(recipe, item);
                if (!InventoryHasEnoughEmptySlots(inventory, recyclingList, item)) continue;
                foreach (var entry in recyclingList)
                    inventory.AddItem(
                        entry.prefab.name, entry.amount, entry.mQuality,
                        entry.mVariant, player.GetPlayerID(), player.GetPlayerName()
                    );
                inventory.RemoveItem(item);
            }
        }

        public static void RecycleOneItemToTargetInventory(ItemDrop.ItemData item, Inventory inventory,
            List<Recipe> recipes, Player player)
        {
            var recipe = recipes
                .FirstOrDefault(r => r.m_item.m_itemData.m_shared.m_name == item.m_shared.m_name);
            if (recipe == null)
            {
                Plugin.Log.LogInfo($"Could not recycle {item.m_shared.m_name}. Could not find a recipe. ");
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                    $"Could not recycle {Localization.instance.Localize(item.m_shared.m_name)}.\n" +
                    "Could not find a valid recipe");
                return;
            }

            //todo: optimize two .GetComponent<ItemDrop> calls 
            var recyclingList = GatherRecyclingResultForItem(recipe, item);
            if (!InventoryHasEnoughEmptySlots(inventory, recyclingList, item)) return;
            foreach (var entry in recyclingList)
                inventory.AddItem(
                    entry.prefab.name, entry.amount, entry.mQuality,
                    entry.mVariant, player.GetPlayerID(), player.GetPlayerName()
                );
            inventory.RemoveItem(item);
        }

        private static bool InventoryHasEnoughEmptySlots(Inventory cInventory, List<RecyclingEntry> recyclingList,
            ItemDrop.ItemData itemData)
        {
            var emptySlotsAmount = cInventory.GetEmptySlots();
            var needsSlots = recyclingList.Sum(entry =>
                Math.Ceiling(entry.amount /
                             (double) entry.prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxStackSize));

            if (emptySlotsAmount >= needsSlots) return true;
            Debug.Log($"Not enough slots to recycle {itemData.m_shared.m_name}");
            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                $"Could not recycle {Localization.instance.Localize(itemData.m_shared.m_name)}.\n" +
                $"Need {needsSlots} slots but only {emptySlotsAmount} were available");
            return false;
        }

        private static List<RecyclingEntry> GatherRecyclingResultForItem(Recipe recipe, ItemDrop.ItemData itemData)
        {
            var recyclingList = new List<RecyclingEntry>();
            var recyclingRate = Plugin.Settings.RecyclingRate.Value;
            Plugin.Log.LogDebug($"Gathering recycling result for {itemData.m_shared.m_name}");
            var amountToCraftedRecipeAmountPercentage = itemData.m_stack / (double)recipe.m_amount;
            foreach (var resource in recipe.m_resources)
            {
                var rItemData = resource.m_resItem.m_itemData;
                var preFab = ObjectDB.instance.m_items.FirstOrDefault(item =>
                    item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name == rItemData.m_shared.m_name);
                if (preFab == null)
                {
                    Plugin.Log.LogWarning(
                        $"Could not find a prefab for {itemData.m_shared.m_name}! Won't be able to spawn items");
                    break;
                }

                var amount = Enumerable.Range(1, itemData.m_quality)
                    .Select(level => resource.GetAmount(level))
                    .Sum();
                
                var stackCompensated = amount * amountToCraftedRecipeAmountPercentage;
                var realAmount = Math.Floor(stackCompensated * recyclingRate);
                var finalAmount = (int)realAmount;
                if (realAmount < 1 && itemData.m_shared.m_maxStackSize == 1
                                   && Plugin.Settings.UnstackableItemsAlwaysReturnAtLeastOneResource.Value)
                    finalAmount = 1;
                Plugin.Log.LogDebug("Calculations report.\n" +
                                    $" = = = Input: REA:{resource.m_amount} IQ:{itemData.m_quality} STK:{itemData.m_stack}({itemData.m_shared.m_maxStackSize}) SC:{stackCompensated} ATCRAP:{amountToCraftedRecipeAmountPercentage} A:{amount}, RA:{realAmount}: FA:{finalAmount}");
                recyclingList.Add(
                    new RecyclingEntry(preFab, finalAmount, rItemData.m_quality, rItemData.m_variant));
            }

            return recyclingList;
        }
    }
}
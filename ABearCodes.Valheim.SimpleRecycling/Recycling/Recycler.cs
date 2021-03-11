using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABearCodes.Valheim.SimpleRecycling.Recycling
{
    public static class Recycler
    {
        public static void RecycleInventoryForAllRecipes(Inventory inventory, Player player)
        {
            var itemListSnapshot = new List<ItemDrop.ItemData>();
            // copy the inventory, otherwise collection will constantly change causing issues
            itemListSnapshot.AddRange(inventory.GetAllItems());
            var analysisList = new List<RecyclingAnalysisContext>();
            for (var index = 0; index < itemListSnapshot.Count; index++)
            {
                var item = itemListSnapshot[index];
                var analysisContext = new RecyclingAnalysisContext(item);
                analysisList.Add(analysisContext);
                RecycleOneItemInInventory(analysisContext, inventory, player);
                if (analysisContext.ShouldErrorDumpAnalysis || Plugin.Settings.DebugAlwaysDumpAnalysisContext.Value)
                {
                    analysisContext.Dump();
                }
            }

            var stringBuilder = new StringBuilder();
            foreach (var analysisContext in analysisList.Where(analysis => analysis.RecyclingImpediments.Count > 0))
            {
                stringBuilder.AppendLine($"Could not recycle {Plugin.Localize(analysisContext.Item.m_shared.m_name)} " +
                                         $"for {analysisContext.RecyclingImpediments.Count} reasons:");
                foreach (var impediment in analysisContext.RecyclingImpediments) stringBuilder.AppendLine(impediment);
            }

            if (stringBuilder.ToString().Length == 0 || !Plugin.Settings.NotifyOnSalvagingImpediments.Value) return;
            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, stringBuilder.ToString());
        }

        public static List<RecyclingAnalysisContext> GetRecyclingAnalysisForInventory(Inventory inventory, Player player)
        {
            var itemListSnapshot = new List<ItemDrop.ItemData>();
            // copy the inventory, otherwise collection will constantly change causing issues
            itemListSnapshot.AddRange(inventory.GetAllItems());
            var analysisList = new List<RecyclingAnalysisContext>();
            for (var index = 0; index < itemListSnapshot.Count; index++)
            {
                var item = itemListSnapshot[index];
                var analysisContext = new RecyclingAnalysisContext(item);
                analysisList.Add(analysisContext);
                TryAnalyzeOneItem(analysisContext, inventory, player);
            }
            return analysisList;
        }

        private static void RecycleOneItemInInventory(RecyclingAnalysisContext analysisContext, Inventory inventory,
            Player player)
        {
            if (!TryAnalyzeOneItem(analysisContext, inventory, player)) return;

            if (analysisContext.RecyclingImpediments.Count > 0)
                return;
            DoInventoryChanges(analysisContext, inventory, player);
        }

        public static bool TryAnalyzeOneItem(RecyclingAnalysisContext analysisContext, Inventory inventory, Player player)
        {
            if (!TryFindRecipeForItem(analysisContext, player)) return false;
            //todo: optimize two .GetComponent<ItemDrop> calls 
            AnalyzeMaterialYieldForItem(analysisContext);
            AnalyzeInventoryHasEnoughEmptySlots(analysisContext, inventory);
            AnalyzeItemDisplayImpediments(analysisContext, inventory, player);
            return true;
        }
                
        private static void AnalyzeItemDisplayImpediments(RecyclingAnalysisContext analysisContext, Inventory inventory, Player player)
        {
            if (player.GetInventory() != inventory) return;

            if (analysisContext.Item.m_equiped && Plugin.Settings.HideEquippedItemsInRecyclingTab.Value) 
                analysisContext.DisplayImpediments.Add("Item is currently equipped");

            if(analysisContext.Item.m_gridPos.y == 0 && Plugin.Settings.IgnoreItemsOnHotbar.Value)
                analysisContext.DisplayImpediments.Add("Item is on hotbar and setting to ignore hotbar is set");

            if (analysisContext.Recipe.m_craftingStation?.m_name != null
                && Plugin.Settings.StationFilterEnabled.Value
                && Plugin.Settings.StationFilterList.Contains(analysisContext.Recipe.m_craftingStation.m_name))
                analysisContext.DisplayImpediments.Add(
                    $"Item is from filtered station ({Plugin.Localize(analysisContext.Recipe.m_craftingStation.m_name)})");
        }

        public static void DoInventoryChanges(RecyclingAnalysisContext analysisContext, Inventory inventory, Player player)
        {
            Plugin.Log.LogDebug($"Inventory changes requested");
            foreach (var entry in analysisContext.Entries)
            {
                if (entry.Amount == 0 && entry.InitialRecipeHadZero) continue;
                var addedItem = inventory.AddItem(
                    entry.Prefab.name, entry.Amount, entry.mQuality,
                    entry.mVariant, player.GetPlayerID(), player.GetPlayerName()
                );
                if (addedItem != null)
                {
                    Plugin.Log.LogDebug($"Added {entry.Amount} of {entry.Prefab.name}");
                    continue;
                }
                
                if (entry.Amount < 1 && !Plugin.Settings.PreventZeroResourceYields.Value)
                {
                    Plugin.Log.LogDebug("Adding item failed, but player disabled zero resource yields prevention, item loss expected. ");
                    continue;
                }
                Plugin.Log.LogError(
                    "Inventory refused to add item after valid analysis! Check the error from the inventory for details. Will mark analysis for dumping.");
                analysisContext.ShouldErrorDumpAnalysis = true;
                analysisContext.RecyclingImpediments.Add(
                    $"Inventory could not add item {Plugin.Localize(entry.Prefab.name)}");
            }

            if (inventory.RemoveItem(analysisContext.Item))
            {
                Plugin.Log.LogDebug($"Removed item {analysisContext.Item.m_shared.m_name}");
                return;
            }
            Plugin.Log.LogError(
                "Inventory refused to remove item after valid analysis! Check the error from the inventory for details. Will mark analysis for dumping.");
            analysisContext.ShouldErrorDumpAnalysis = true;
            analysisContext.RecyclingImpediments.Add($"Inventory could not remove item {Plugin.Localize(analysisContext.Item.m_shared.m_name)}");
        }

        private static bool TryFindRecipeForItem(RecyclingAnalysisContext analysisContext, Player player)
        {
            var item = analysisContext.Item;
            var foundRecipes = ObjectDB.instance.m_recipes
                // some recipes are just weird, so check for null item, data and even shared (somehow it happens)
                .Where(rec => rec?.m_item?.m_itemData?.m_shared?.m_name == item.m_shared.m_name)
                .ToList();
            if (foundRecipes.Count == 0)
            {
                Plugin.Log.LogDebug($"Could not find a recipe for {item.m_shared.m_name}");
                analysisContext.DisplayImpediments.Add($"Could not find a recipe for {item.m_shared.m_name}");
                analysisContext.Recipe = null;
                return false;
            }
            if (foundRecipes.Count > 1)
            {
                analysisContext.RecyclingImpediments.Add(
                    $"Found multiple recipes ({foundRecipes.Count}) for {Localization.instance.Localize(item.m_shared.m_name)}");
                analysisContext.DisplayImpediments.Add($"Found multiple recipes ({foundRecipes.Count}) for {Localization.instance.Localize(item.m_shared.m_name)}");
                analysisContext.Recipe = null;
                return false;
            }
            
            analysisContext.Recipe = foundRecipes.FirstOrDefault();
            if (!player.IsRecipeKnown(analysisContext.Recipe.m_item.m_itemData.m_shared.m_name) &&
                !Plugin.Settings.AllowRecyclingUnknownRecipes.Value)
            {
                analysisContext.RecyclingImpediments.Add(
                    $"Recipe for {Localization.instance.Localize(item.m_shared.m_name)} not known.");
            }
            return true;
        }

        private static void AnalyzeInventoryHasEnoughEmptySlots(RecyclingAnalysisContext analysisContext,
            Inventory inventory)
        {
            var emptySlotsAmount = inventory.GetEmptySlots();
            var needsSlots = analysisContext.Entries.Sum(entry =>
                Math.Ceiling(entry.Amount /
                             (double) entry.Prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxStackSize));

            if (emptySlotsAmount >= needsSlots) return;
            analysisContext.RecyclingImpediments.Add($"Need {needsSlots} slots but only {emptySlotsAmount} were available");
        }

        private static void AnalyzeMaterialYieldForItem(RecyclingAnalysisContext analysisContext)
        {
            var recyclingRate = Plugin.Settings.RecyclingRate.Value;
            var itemData = analysisContext.Item;
            var recipe = analysisContext.Recipe;
            // Plugin.Log.LogDebug($"Gathering recycling result for {itemData.m_shared.m_name}");
            var amountToCraftedRecipeAmountPercentage = itemData.m_stack / (double) recipe.m_amount;
            foreach (var resource in recipe.m_resources)
            {
                var rItemData = resource.m_resItem.m_itemData;
                var preFab = ObjectDB.instance.m_items.FirstOrDefault(item =>
                    item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name == rItemData.m_shared.m_name);
                if (preFab == null)
                {
                    Plugin.Log.LogWarning(
                        $"Could not find a prefab for {itemData.m_shared.m_name}! Won't be able to spawn items. You might want to report this!");
                    analysisContext.RecyclingImpediments.Add(
                        $"Could not find item {Localization.instance.Localize(itemData.m_shared.m_name)}({itemData.m_shared.m_name})");
                    continue;
                }

                var (finalAmount, initialRecipeHadZero) = CalculateFinalAmount(itemData, resource, amountToCraftedRecipeAmountPercentage,
                    recyclingRate);
                analysisContext.Entries.Add(
                    new RecyclingAnalysisContext.RecyclingYieldEntry(preFab, rItemData, finalAmount, rItemData.m_quality,
                        rItemData.m_variant, initialRecipeHadZero));
                if (Plugin.Settings.PreventZeroResourceYields.Value && finalAmount == 0 && !initialRecipeHadZero)
                {
                    analysisContext.RecyclingImpediments.Add(
                        $"Recycling would yield 0 of {Localization.instance.Localize(resource.m_resItem.m_itemData.m_shared.m_name)}");
                }
            }
        }

        private static (int Amount, bool InitialRecipeHadZero) CalculateFinalAmount(ItemDrop.ItemData itemData, Piece.Requirement resource,
            double amountToCraftedRecipeAmountPercentage, float recyclingRate)
        {
            var amountPerLevelSum = Enumerable.Range(1, itemData.m_quality)
                .Select(level => resource.GetAmount(level))
                .Sum();
            if (amountPerLevelSum == 0) return (Amount: 0, InitialRecipeHadZero: true);
            var stackCompensated = amountPerLevelSum * amountToCraftedRecipeAmountPercentage;
            var realAmount = Math.Floor(stackCompensated * recyclingRate);
            var finalAmount = (int) realAmount;
            if (realAmount < 1 && itemData.m_shared.m_maxStackSize == 1
                               && Plugin.Settings.UnstackableItemsAlwaysReturnAtLeastOneResource.Value)
                finalAmount = 1;
            if(Plugin.Settings.DebugAllowSpammyLogs.Value)
            Plugin.Log.LogDebug("Calculations report.\n" +
                                $" = = = {resource.m_resItem.m_itemData.m_shared.m_name} - " +
                                $"REA:{resource.m_amount} APLS: {amountPerLevelSum} IQ:{itemData.m_quality} " +
                                $"STK:{itemData.m_stack}({itemData.m_shared.m_maxStackSize}) SC:{stackCompensated} " +
                                $"ATCRAP:{amountToCraftedRecipeAmountPercentage} A:{amountPerLevelSum}, " +
                                $"RA:{realAmount} FA:{finalAmount}");
            return (Amount: finalAmount, InitialRecipeHadZero: false);
        }
    }
}
namespace ABearCodes.Valheim.CraftingWithContainers.Smelting
{
    public static class SmelterUtils
    {
        public static ItemDrop.ItemData FindCookableItemFiltered(this Smelter smelter, Inventory inventory)
        {
            Plugin.Log.LogDebug($"Searching {smelter.m_conversion.Count} conversions for {smelter.m_name}");
            foreach (var itemConversion in smelter.m_conversion)
            {
                if (Plugin.Settings.AllowedFuelsAsList.Count != 0
                    && !Plugin.Settings.AllowedFuelsAsList.Contains(itemConversion.m_from.m_itemData.m_shared.m_name))
                {
                    Plugin.Log.LogDebug($"{itemConversion.m_from.m_itemData.m_shared.m_name} not allowed in settings ");
                    continue;
                }

                var itemData = inventory.GetItem(itemConversion.m_from.m_itemData.m_shared.m_name);
                if (itemData == null) continue;
                return itemData;
            }

            return null;
        }
    }
}
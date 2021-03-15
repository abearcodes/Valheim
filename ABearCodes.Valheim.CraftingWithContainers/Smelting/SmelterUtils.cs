using System.Collections.Generic;


namespace ABearCodes.Valheim.CraftingWithContainers.Smelting
{
    public static class SmelterUtils
    {
        public static ItemDrop.ItemData FindCookableItemFiltered(this Smelter smelter, Inventory inventory, List<string> filter)
        {
            foreach (var itemConversion in smelter.m_conversion)
            {
                if (filter.Count != 0 && !filter.Contains(itemConversion.m_from.m_itemData.m_shared.m_name))
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
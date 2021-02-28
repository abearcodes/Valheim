namespace ABearCodes.Valheim.CraftingWithContainers.Utils
{
    public static class InventoryUtils
    {
        /// <summary>
        /// Removes as much as possible from an inventory and returns the amount left to remove
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="name"></param>
        /// <param name="leftToRemove"></param>
        /// <returns></returns>
        public static int RemoveItemAsMuchAsPossible(this Inventory inventory, string name, int leftToRemove)
        {
            var currentInventoryCount = inventory.CountItems(name);
            var itemsToTake = currentInventoryCount < leftToRemove
                ? currentInventoryCount
                : leftToRemove;
            inventory.RemoveItem(name, itemsToTake);
            leftToRemove -= itemsToTake;
            Plugin.Log.LogDebug($"Removed {itemsToTake} from inv {inventory.GetHashCode()}");
            return leftToRemove;
        }
    }
}
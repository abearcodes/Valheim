namespace ABearCodes.Valheim.CraftingWithContainers.Utils
{
    public static class InventoryUtils
    {
        /// <summary>
        /// Removes as much as possible from an inventory and returns the amount left to remove
        /// </summary>
        /// <returns>amount of items taken</returns>
        public static int RemoveItemAsMuchAsPossible(this Inventory inventory, string name, int requestedAmount)
        {
            var currentInventoryCount = inventory.CountItems(name);
            var itemsToTake = currentInventoryCount < requestedAmount
                ? currentInventoryCount
                : requestedAmount;
            inventory.RemoveItem(name, itemsToTake);
            Plugin.Log.LogDebug($"Removed {itemsToTake} from inv {inventory.GetHashCode()}");
            return itemsToTake;
        }
    }
}
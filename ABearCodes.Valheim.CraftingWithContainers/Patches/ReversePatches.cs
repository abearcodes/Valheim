using System;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Patches
{
    [HarmonyPatch]
    public static class ReversePatches
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "CountItems")]
        public static int InventoryCountItems(Inventory __instance, string name)
        {
            // We override the default behaviour but we still want to the initial implementation
            throw new NotImplementedException("Stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(string), typeof(int))]
        public static void InventoryRemoveItemByString(Inventory __instance, string name, int amount)
        {
            // We override the default behaviour but we still want to the initial implementation
            throw new NotImplementedException("Stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Container), "CheckAccess", typeof(long))]
        public static bool CheckAccess(this Container instance, long playerID)
        {
            // Container.CheckAccess is private by default
            throw new NotImplementedException("Stub");
        }
        
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Container), "Save")]
        public static void Save(this Container instance)
        {
            throw new NotImplementedException("Stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "Changed")]
        public static void Changed(this Inventory instance)
        {
            throw new NotImplementedException("Stub");
        }
        
        
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(InventoryGui), "UpdateCraftingPanel")]
        public static void UpdateCraftingPanel(this InventoryGui instance, bool focus)
        {
            // Container.Changed is private by default
            throw new NotImplementedException("Stub");
        }
    }
}
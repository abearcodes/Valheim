using System;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Patches
{
    [HarmonyPatch]
    public static class ReversePatches
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "CountItems")]
        public static int CountItemsOriginal(this Inventory __instance, string name)
        {
            throw new NotImplementedException("Stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(string), typeof(int))]
        public static void RemoveItemOriginal(this Inventory __instance, string name, int amount)
        {
            throw new NotImplementedException("Stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Container), "CheckAccess", typeof(long))]
        public static bool CheckAccess(this Container instance, long playerID)
        {
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
            throw new NotImplementedException("Stub");
        }
    }
}
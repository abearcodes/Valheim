using System;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Common
{
    [HarmonyPatch]
    public static class ReversePatches
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(global::Inventory), "CountItems")]
        public static int CountItemsOriginal(this global::Inventory __instance, string name)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(global::Inventory), "RemoveItem", typeof(string), typeof(int))]
        public static void RemoveItemOriginal(this global::Inventory __instance, string name, int amount)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Container), "CheckAccess", typeof(long))]
        public static bool CheckAccess(this Container instance, long playerID)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Container), "Save")]
        public static void Save(this Container instance)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(global::Inventory), "Changed")]
        public static void Changed(this global::Inventory instance)
        {
            throw new NotImplementedException("stub");
        }


        [HarmonyReversePatch]
        [HarmonyPatch(typeof(global::Inventory), "HaveItem", typeof(string))]
        public static bool HaveItemOriginal(this global::Inventory instance, string name)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(InventoryGui), "UpdateCraftingPanel")]
        public static void UpdateCraftingPanel(this InventoryGui instance, bool focus)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Chat), "AddString",
            typeof(string), typeof(string), typeof(Talker.Type))]
        public static void AddString(this Chat instance, string user, string text, Talker.Type type)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Smelter), "GetFuel")]
        public static float GetFuel(this Smelter instance)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Smelter), "FindCookableItem", typeof(global::Inventory))]
        public static ItemDrop.ItemData FindCookableItem(this Smelter instance, global::Inventory inventory)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Smelter), "IsItemAllowed", typeof(string))]
        public static bool IsItemAllowed(this Smelter instance, string itemName)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Smelter), "GetQueueSize")]
        public static int GetQueueSize(this Smelter instance)
        {
            throw new NotImplementedException("");
        }
    }
}
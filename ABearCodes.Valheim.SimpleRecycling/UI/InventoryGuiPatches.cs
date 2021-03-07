using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ABearCodes.Valheim.SimpleRecycling.UI
{
    [HarmonyPatch]
    public static class InventoryGuiPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InventoryGui), "OnTabCraftPressed")]
        [HarmonyPatch(typeof(InventoryGui), "OnTabUpgradePressed")]
        static void OnTabCraftPressedAlsoEnableRecycling(InventoryGui __instance)
        {
            Plugin.RecyclingTabButtonHolder.SetInteractable(true);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InventoryGui), "UpdateCraftingPanel")]
        static bool UpdateCraftingPanelDetourOnRecyclingTab(InventoryGui __instance)
        {
            if (Plugin.RecyclingTabButtonHolder.InRecycleTab()) return false;
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryGui), "UpdateCraftingPanel")]
        static void UpdateCraftingPanelDetourOnOtherTabsEnableRecyclingButton(InventoryGui __instance)
        {
            if (Plugin.RecyclingTabButtonHolder.InRecycleTab()) return;
            var player = Player.m_localPlayer;
            Plugin.RecyclingTabButtonHolder.SetInteractable(true);
            if (!player.GetCurrentCraftingStation() && !player.NoCostCheat())
            {
                Plugin.RecyclingTabButtonHolder.SetActive(false);
                return;
            }
            Plugin.RecyclingTabButtonHolder.SetActive(true);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InventoryGui), "UpdateRecipe", typeof(Player), typeof(float))]
        static bool UpdateRecipeOnRecyclingTab(InventoryGui __instance, Player player, float dt)
        {
            if (!Plugin.RecyclingTabButtonHolder.InRecycleTab()) return true;
            Plugin.RecyclingTabButtonHolder.UpdateRecipe(player, dt);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Inventory), "Changed")]
        static void InventorySave(Inventory __instance)
        {
            if (!Plugin.RecyclingTabButtonHolder.InRecycleTab()) return;
            Plugin.RecyclingTabButtonHolder.UpdateCraftingPanel();
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(InventoryGui), "SetRecipe", typeof(int), typeof(bool))]
        public static void SetRecipe(this InventoryGui __instance, int index, bool center)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(InventoryGui), "GetSelectedRecipeIndex")]
        public static int GetSelectedRecipeIndex(this InventoryGui __instance)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(InventoryGui), "OnSelectedRecipe", typeof(GameObject))]
        public static void OnSelectedRecipe(this InventoryGui __instance, GameObject button)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(InventoryGui), "FindSelectedRecipe", typeof(GameObject))]
        public static int FindSelectedRecipe(this InventoryGui __instance, GameObject button)
        {
            throw new NotImplementedException("stub");
        }
        



    }
}
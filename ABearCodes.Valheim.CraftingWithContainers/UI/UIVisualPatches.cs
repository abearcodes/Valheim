using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ABearCodes.Valheim.CraftingWithContainers.UI
{
    [HarmonyPatch]
    public class UIVisualPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InventoryGui), "SetupRequirement",
            typeof(Transform), typeof(Piece.Requirement),
            typeof(Player), typeof(bool), typeof(int))]
        private static bool PatchRequirementAmountIndicator(Transform elementRoot,
            Piece.Requirement req, Player player, bool craft, int quality)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value ||
                !Plugin.Settings.ModifyItemCountIndicator.Value)
                return true;
            var component1 = elementRoot.transform.Find("res_icon").GetComponent<Image>();
            var component2 = elementRoot.transform.Find("res_name").GetComponent<Text>();
            var component3 = elementRoot.transform.Find("res_amount").GetComponent<Text>();
            var component4 = elementRoot.GetComponent<UITooltip>();
            if (req.m_resItem == null) return true;
            component1.gameObject.SetActive(true);
            component2.gameObject.SetActive(true);
            component3.gameObject.SetActive(true);
            component1.sprite = req.m_resItem.m_itemData.GetIcon();
            component1.color = Color.white;
            component4.m_text = Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name);
            component2.text = Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name);
            var num = player.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name);
            var amount = req.GetAmount(quality);
            if (amount <= 0)
            {
                InventoryGui.HideRequirement(elementRoot);
                return false;
            }

            component3.text = string.Format($"{amount}/{num}");
            if (num < amount)
                component3.color = (double) Mathf.Sin(Time.time * 10f) > 0.0 ? Color.red : Color.white;
            else
                component3.color = Color.white;
            return true;
        }

        [HarmonyPatch(typeof(InventoryGui), "DoCrafting", typeof(Player))]
        [HarmonyPrefix]
        private static void PatchDoCraftingHook(Player player, Recipe ___m_craftRecipe)
        {
            if (!Plugin.Settings.LogItemRemovalsToConsole.Value) return;
            Console.instance.Print(
                $"<color=orange>{player.GetPlayerName()}</color> crafting <color=orange>{___m_craftRecipe.name}</color>");
        }
    }
}
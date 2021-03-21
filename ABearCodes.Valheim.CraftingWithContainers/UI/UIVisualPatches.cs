using System;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Common;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
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
        private static bool SetupRequirementTotalItemsIndicatorPatch(Transform elementRoot,
            Piece.Requirement req, Player player, bool craft, int quality, ref bool __result)
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
                __result = false;
                return false;
            }

            component3.text = string.Format($"{amount}/{num}");
            if (num < amount)
                component3.color = (double) Mathf.Sin(Time.time * 10f) > 0.0 ? Color.red : Color.white;
            else
                component3.color = Color.white;
            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(Hud), "SetupPieceInfo", typeof(Piece))]
        private static void Postfix(Piece piece, Text ___m_buildSelection)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value || piece == null ||
                piece.m_name == "$piece_repair") return;
            var containers = ContainerTracker.GetViableContainersInRangeForPlayer(Player.m_localPlayer,
                Plugin.Settings.ContainerLookupRange.Value);
            var maxBuilds = piece.m_resources.Select(resource =>
            {
                var playerCount = Player.m_localPlayer.GetInventory()
                    .CountItemsOriginal(resource.m_resItem.m_itemData.m_shared.m_name);
                var containerCount = containers.Sum(container =>
                    container.Container.GetInventory()
                        .CountItemsOriginal(resource.m_resItem.m_itemData.m_shared.m_name));
                return (playerCount + containerCount) / resource.m_amount;
            }).AddItem(Int32.MaxValue)
            .Min();
            ___m_buildSelection.text =
                $"{Localization.instance.Localize(piece.m_name)} (<color=white>{(maxBuilds == int.MaxValue? "∞" : maxBuilds.ToString())}</color>)";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using ABearCodes.Valheim.CraftingWithContainers.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ABearCodes.Valheim.CraftingWithContainers.Crafting
{
    [HarmonyPatch]
    public class UIVisualPatches
    {
        [HarmonyPatch(typeof(InventoryGui), "SetupRequirement",
            typeof(Transform), typeof(Piece.Requirement),
            typeof(Player), typeof(bool), typeof(int))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> PatchRequirementAmountIndicator(
            IEnumerable<CodeInstruction> instructions)
        {
            HarmonyUtils.LogDebugBuildOnly("Transpiler patch: SetupRequirement");

            var searchTemplate = new List<Tuple<int, CodeInstruction>>
            {
                new Tuple<int, CodeInstruction>(0, new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(InventoryGui), "HideRequirement",
                        new[] {typeof(Transform)}))),
                new Tuple<int, CodeInstruction>(3, new CodeInstruction(OpCodes.Ldloc_2)),
                new Tuple<int, CodeInstruction>(5, new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(int), "ToString"))),
                new Tuple<int, CodeInstruction>(6, new CodeInstruction(OpCodes.Callvirt,
                    AccessTools.PropertySetter(typeof(Text), "text")))
            };

            var patchCount = 0;
            var instructionsList = instructions.ToList();
            for (var i = 0; i < instructionsList.Count; i++)
            {
                if (!instructionsList.DoInstructionsMatchTemplate(i, searchTemplate)) continue;
                HarmonyUtils.LogDebugBuildOnly("Template matched!");
                // ok, we sure we are kinda sure we are patching the right thing
                var numOperandReference = instructionsList[i + 7].operand;
                var amountOperandReference = instructionsList[i + 8].operand;
                instructionsList.RemoveRange(i + 4, 2);
                instructionsList.InsertRange(i + 4, new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldstr, "{0}/{1}"),
                    new CodeInstruction(OpCodes.Ldloc_S, numOperandReference),
                    new CodeInstruction(OpCodes.Box, typeof(int)),
                    new CodeInstruction(OpCodes.Ldloc_S, amountOperandReference),
                    new CodeInstruction(OpCodes.Box, typeof(int)),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), "Format",
                        new[] {typeof(string), typeof(object), typeof(object)}))
                });
                HarmonyUtils.LogDebugBuildOnly("Patch OK");
                patchCount = 1;
            }

            HarmonyUtils.LogDebugBuildOnly($"Patch count {patchCount}");
            if (patchCount == 0)
                Plugin.Log.LogError(
                    "Crafting amount indicator won't work properly");
            return instructionsList;
        }
        
        [HarmonyPatch(typeof(InventoryGui), "DoCrafting", typeof(Player))]
        [HarmonyPrefix]
        private static void PatchDoCraftingHook(Player player, Recipe ___m_craftRecipe)
        {
            if (!Plugin.Settings.LogItemRemovalsToConsole.Value) return;
            Console.instance.Print($"<color=orange>{player.GetPlayerName()}</color> crafting <color=orange>{___m_craftRecipe.name}</color>");
        }
    }
}
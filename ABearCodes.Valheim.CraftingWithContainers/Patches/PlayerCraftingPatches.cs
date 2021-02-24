using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using ABearCodes.Valheim.CraftingWithContainers.Crafting;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Patches
{
    // [HarmonyPatch(typeof(Player))]
    public static class PlayerCraftingPatches
    {
        /// <summary>
        /// Patches the call to Inventory::CountItems(string) to CraftingHandler::CountItemsArea(string)
        /// </summary>
        // [HarmonyPatch("HaveRequirements", typeof(Piece.Requirement[]), typeof(bool), typeof(int))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = instructions.ToList();
            var didPatch = false;
            var resourceGetAmountRef = typeof(Piece.Requirement).GetMethod("GetAmount", new[] {typeof(int)});
            var countItemsRef = typeof(Inventory).GetMethod("CountItems", new[] {typeof(string)});
            for (var i = 0; i < instructionsList.Count(); i++)
            {
                var ins = instructionsList[i];
                if (ins.opcode == OpCodes.Callvirt && ins.OperandIs(resourceGetAmountRef))
                {
                    instructionsList[i + 3] = new CodeInstruction(OpCodes.Nop);
                }

                if (ins.opcode != OpCodes.Callvirt || !ins.OperandIs(countItemsRef))
                    continue;
                didPatch = true;
                // instructionsList[i] = new CodeInstruction(OpCodes.Call,
                //     AccessTools.Method(typeof(CraftingHandler), "CountItemsArea"));
                break;
            }

            if (!didPatch)
                Plugin.Log.LogError(
                    $"Patching of {countItemsRef.ToString()} was not successful. CratingWithContainers will not properly work.");
            var index = 0;
            var sb = new StringBuilder();
            foreach (var codeInstruction in instructionsList)
            {
                sb.AppendLine($"{index}: {codeInstruction}");
                index++;
            }
            Debug.Log(sb.ToString());

            return instructionsList.AsEnumerable();
        }
    }
}


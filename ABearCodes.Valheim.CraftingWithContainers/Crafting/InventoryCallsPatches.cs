using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ABearCodes.Valheim.CraftingWithContainers.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ABearCodes.Valheim.CraftingWithContainers.Crafting
{
    /*
     * Patches a series of calls to Inventory::CountItems/HaveItem/etc via transpiler in various
     * places of the Valheim codebase. 
     *
     * Why not use a prefix/postfix?
     * Pretty much mod compatibility. We want to keep the initial functions completely
     * intact just in case other mods use them. This also gives us finer control
     * over when we apply our magic.
     *
     */
    [HarmonyPatch]
    public static class InventoryCallsPatches
    {
        private static readonly List<Tuple<CodeInstruction, CodeInstruction>> methodPatchMap =
            new List<Tuple<CodeInstruction, CodeInstruction>>
            {
                new Tuple<CodeInstruction, CodeInstruction>(
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.Method(typeof(Inventory), "CountItems", new[] {typeof(string)})),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CraftingHelper), "CountItemsArea"))),
                new Tuple<CodeInstruction, CodeInstruction>(
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.Method(typeof(Inventory), "HaveItem", new[] {typeof(string)})),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CraftingHelper), "HaveItemArea"))),
                new Tuple<CodeInstruction, CodeInstruction>(
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.Method(typeof(Inventory), "RemoveItem", new[] {typeof(string), typeof(int)})),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CraftingHelper), "RemoveItemArea")))
                
            };

        [HarmonyTargetMethods]
        [UsedImplicitly]
        private static IEnumerable<MethodInfo> CountItemsHaveItemsTargets()
        {
            // Crafting / building (count)
            yield return AccessTools.Method(typeof(Player), "HaveRequirements",
                new[] {typeof(Piece.Requirement[]), typeof(bool), typeof(int)});
            yield return AccessTools.Method(typeof(Player), "HaveRequirements",
                new[] {typeof(Piece), typeof(Player.RequirementMode)});
            yield return AccessTools.Method(typeof(InventoryGui), "SetupRequirement",
                new[] {typeof(Transform), typeof(Piece.Requirement), typeof(Player), typeof(bool), typeof(int)});

            // Crafting / building (remove)
            yield return AccessTools.Method(typeof(Player), "ConsumeResources",
                new[] {typeof(Piece.Requirement[]), typeof(int)});

            // Fireplace (count)
            if (Plugin.Settings.AllowTakeFuelForFireplace.Value)
            {
                yield return AccessTools.Method(typeof(Fireplace), "UseItem",
                    new[] {typeof(Humanoid), typeof(ItemDrop.ItemData)});
                yield return AccessTools.Method(typeof(Fireplace), "Interact",
                    new[] {typeof(Humanoid), typeof(bool)});
            }

            // Smelter / Kiln (count)
            if (Plugin.Settings.AllowTakeFuelForKilnAndFurnace.Value)
            {
                yield return AccessTools.Method(typeof(Smelter), "OnAddFuel",
                    new[] {typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData)});
            }

            /*
             * Last updated 0.146.8
             * Considered unimplemented patches:
             *
             * ~InventoryGui.cs : 580
             *     public bool HaveRequirements(Piece piece, Player.RequirementMode mode)
             *     ...
             *     localPlayer.GetInventory().GetAllItems(recipe.m_item.m_itemData.m_shared.m_name, this.m_tempItemList);
             *     ...
             *    Reason: should not upgrade items in containers
             * 
             */
        }

        [HarmonyTranspiler]
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> CountItemsHaveItemsReferencesPatch(MethodBase method,
            IEnumerable<CodeInstruction> instructions)
        {
            HarmonyUtils.LogDebugBuildOnly($"Transpiler patching {method.DeclaringType.Name}::{method}");
            var instructionsList = instructions.ToList();
            var patchCount = 0;
            
            for (var i = instructionsList.Count - 1; i >= 0; i--)
            {
                var ins = instructionsList[i];
                if (ins.opcode != OpCodes.Callvirt) continue;
                var patchPair = methodPatchMap
                    .SingleOrDefault(pair => ins.Is(pair.Item1));
                if (patchPair == null) continue;
                HarmonyUtils.LogDebugBuildOnly("Patching:");
                instructionsList[i] = patchPair.Item2;
                HarmonyUtils.LogDebugBuildOnly("Post patch instruction is:");
                HarmonyUtils.LogDebugBuildOnly($"{i} - {instructionsList[i]}");
                patchCount += 1;
            }

            HarmonyUtils.LogDebugBuildOnly($"Patch count {patchCount}");
            if (patchCount == 0){
                Plugin.Log.LogError(
                    "Counting/deducting patching was not successful. CratingWithContainers will not work properly if at all.\n" +
                    $"Dumping initial instruction list:\n{string.Join("\n", instructionsList.Select((instruction, i) => $" {i} - {instruction}"))}");
            }
            
            return instructionsList;
        }
    }
}
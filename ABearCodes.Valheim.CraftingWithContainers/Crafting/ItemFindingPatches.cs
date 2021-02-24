using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Crafting
{
    
    [HarmonyPatch]
    public static class ItemFindingPatches
    {
        /// <summary>
        ///     Patches the call to Inventory::CountItems(string) to CraftingHandler::CountItemsArea(string)
        /// </summary>
        [HarmonyPatch(typeof(Player), "HaveRequirements", typeof(Piece.Requirement[]), typeof(bool), typeof(int))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TranspileHaveRequirements(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = instructions.ToList();
            var didPatch = false;
            var humanoidInventoryRef =
                typeof(Humanoid).GetField("m_inventory", BindingFlags.Instance | BindingFlags.NonPublic);
            var countItemsRef = typeof(Inventory).GetMethod("CountItems", new[] {typeof(string)});
            for (var i = 0; i < instructionsList.Count(); i++)
            {
                var ins = instructionsList[i];
                if (ins.opcode == OpCodes.Ldfld && ins.OperandIs(humanoidInventoryRef))
                    instructionsList[i] = new CodeInstruction(OpCodes.Nop);

                if (ins.opcode != OpCodes.Callvirt || !ins.OperandIs(countItemsRef)) continue;
                instructionsList[i] = new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(CraftingHelper), "CountItemsArea"));
                didPatch = true;
                break;
            }

            if (!didPatch)
                Plugin.Log.LogError(
                    "Patching of HaveRequirements was not successful. CratingWithContainers will not work properly if at all.");
            return instructionsList.AsEnumerable();
        }


    }
}
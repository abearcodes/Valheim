using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Utils
{
    public static class HarmonyUtils
    {
        /// <summary>
        /// Compares code instructions. If compared operands are null returns true (default .Is() method throws exception)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool Is(this CodeInstruction self, CodeInstruction other)
        {
            return self.opcode == other.opcode &&
                   (self.operand == null && other.operand == null || self.OperandIs(other.operand));
        }

        /// <summary>
        /// Matches instructions from this list against another list from the provided starting index 
        /// </summary>
        /// <param name="instructions">instructions to search through</param>
        /// <param name="startingIndex">starting index to match from</param>
        /// <param name="templateInstructions">list of index and CodeInstruction pairs to compare against</param>
        /// <returns></returns>
        public static bool DoInstructionsMatchTemplate(this List<CodeInstruction> instructions, int startingIndex,
            List<Tuple<int, CodeInstruction>> templateInstructions)
        {
            var res = true;
            foreach (var template in templateInstructions)
            {
                var targetIndex = startingIndex + template.Item1;
                if (targetIndex >= instructions.Count || targetIndex < 0) return false;
                if (!instructions[targetIndex].Is(template.Item2))
                {
                    res = false;
                    break;
                }
                LogDebugBuildOnly($"Matched:\n" +
                                  $"{targetIndex} - {instructions[targetIndex]}\n" +
                                  $"+{template.Item1} - {template.Item2}");
            }
            return res;
        }
        
        public static void LogDebugBuildOnly(string str)
        {
#if DEBUG
            Plugin.Log.LogDebug(str);
#endif
        }
    }
}
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Patches
{
    public static class CompatibilityFixer
    {
        public static void Apply(Harmony harmony)
        {
            RemoveAllPrefixesFrom_InventoryGui_SetupRequirement(harmony);
        }

        /*
         * Prefixes on SetupRequirement usually completely reroute the method.
         * So we need to remove those. Originally caused by ValheimPlus.
         * Might need tweaking if other mods use a SetupRequirement Prefix without
         * removing control completely. 
         */
        private static void RemoveAllPrefixesFrom_InventoryGui_SetupRequirement(Harmony harmony)
        {
            var invSetupReqRef = AccessTools.Method(typeof(InventoryGui), "SetupRequirement");
            var patchInfo = Harmony.GetPatchInfo(invSetupReqRef);
            if (patchInfo == null) return;
            foreach (var prefix in patchInfo.Prefixes)
            {
                harmony.Unpatch(invSetupReqRef, HarmonyPatchType.Prefix, prefix.owner);
            }
        }
    }
}
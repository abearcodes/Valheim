using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers
{
    [BepInPlugin("com.github.abearcodes.valheim.craftingwithcontainers", 
        "Crafting with Containers plugin",
        "1.0.0.0")]
    public partial class Plugin : BaseUnityPlugin
    {
        public Plugin()
        {
            Log = Logger;
        }

        public static ManualLogSource Log { get; private set; }
        
        private void Awake()
        {
            Plugin.Settings.BindConfig(Config);
            var harmony = new Harmony("ABearCodes.CraftingWithContainers");
            harmony.PatchAll();
        }
    }
}
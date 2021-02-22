using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers
{
    [BepInPlugin("com.github.abearcodes.valheim.craftingwithcontainers", 
        "Crafting with Containers",
        "1.0.2")]
    public partial class Plugin : BaseUnityPlugin
    {
        public Plugin()
        {
            Log = Logger;
        }

        public static ManualLogSource Log { get; private set; }
        
        private void Awake()
        {
            Settings.BindConfig(Config);
            var harmony = new Harmony("ABearCodes.Valheim.CraftingWithContainers");
            harmony.PatchAll();
        }
    }
}
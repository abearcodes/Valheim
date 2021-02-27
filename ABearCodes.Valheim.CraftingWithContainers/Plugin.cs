using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using UnityEngine;


namespace ABearCodes.Valheim.CraftingWithContainers
{
    [BepInPlugin("com.github.abearcodes.valheim.craftingwithcontainers",
        "Crafting with Containers",
        "1.0.4")]
    public partial class Plugin : BaseUnityPlugin
    {
        public Plugin()
        {
            Log = Logger;
        }

        public static ManualLogSource Log { get; private set; }
        public static PluginSettings Settings { get; private set; }

        private void Awake()
        {
            Settings = new PluginSettings(Config);
#if DEBUG
            HarmonyFileLog.Enabled = true;   
#endif
            var harmony = new Harmony("ABearCodes.Valheim.CraftingWithContainers");
            harmony.PatchAll();
        }

        private void OnGUI()
        {
            if (!Settings.DebugViableContainerIndicatorEnabled.Value || !Player.m_localPlayer) return;
            foreach (var containerEntry in ContainerTracker.GetViableContainersInRangeForPlayer(Player.m_localPlayer, Settings.ContainerLookupRange.Value))
            {
                var position = Camera.main.WorldToScreenPoint(containerEntry.Container.transform.position + Vector3.up * 0.5f);
                if (position.z <= 0.1) continue;
                GUI.color = Color.magenta;
                var textSize = GUI.skin.label.CalcSize(new GUIContent("+"));
                GUI.Label(new Rect(position.x, Screen.height - position.y, textSize.x, textSize.y), "+");
            }
        }
    }
}
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
        "1.0.13")]
    [BepInDependency("org.bepinex.plugins.valheim_plus", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony _harmony;

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
            _harmony = new Harmony("ABearCodes.Valheim.CraftingWithContainers");
            _harmony.PatchAll();
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F8)) ContainerTracker.ForceScanContainers();
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }

        private void OnGUI()
        {
            if (!Settings.DebugViableContainerIndicatorEnabled.Value || !Player.m_localPlayer) return;
            foreach (var containerEntry in ContainerTracker.GetViableContainersInRangeForPlayer(Player.m_localPlayer,
                Settings.ContainerLookupRange.Value))
            {
                var position =
                    Camera.main.WorldToScreenPoint(containerEntry.Container.transform.position + Vector3.up * 0.5f);
                if (position.z <= 0.1) continue;
                GUI.color = Color.magenta;
                var textToShow = !Settings.DebugViableContainerIndicatorDetailedEnabled.Value? "+"
                    : $"inv#:{containerEntry.Container.GetInventory().GetHashCode()}\n" +
                      $"(uid:{containerEntry.ZNetView.GetZDO().m_uid.id})\n" +
                      $"(own:{containerEntry.ZNetView.GetZDO().m_owner})";
                var textSize = GUI.skin.label.CalcSize(new GUIContent(textToShow));
                GUI.Label(new Rect(position.x, Screen.height - position.y, textSize.x, textSize.y), textToShow);
            }
        }
    }
}
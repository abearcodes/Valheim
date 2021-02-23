using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Tracking
{
    [HarmonyPatch(typeof(Container))]
    internal class ContainerLifecyclePatches
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void Awake(Container __instance, ZNetView ___m_nview)
        {
            // Placement ghost or some other thing, either we it's not a proper network object
            if (___m_nview == null || ___m_nview.GetZDO()?.m_uid == null) return;
            var networkExtension = new ContainerCraftingNetworkExtension(__instance, ___m_nview);
            networkExtension.Register();
            Tracker.AllContainers.Add(new Tracker.ContainerEntry(__instance, networkExtension));
        }

        [HarmonyPatch("OnDestroyed")]
        [HarmonyPostfix]
        private static void OnDestroyed(Container __instance)
        {
            Plugin.Log.LogDebug($"{__instance?.gameObject.name} is destroyed");
            var storedEntry = Tracker.AllContainers.Find(entry => entry.Container == __instance);
            storedEntry.ContainerCraftingNetworkExtension.Unregister();
            Tracker.AllContainers.RemoveAll(entry => entry.Container == __instance);
        }
    }
}
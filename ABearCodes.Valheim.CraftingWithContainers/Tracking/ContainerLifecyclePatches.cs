using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Tracking
{
    [HarmonyPatch(typeof(Container))]
    internal class ContainerLifecyclePatches
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void Awake(Container __instance, ZNetView ___m_nview)
        {
            // if no ZNetView is present then we are dealing with a
            // placement ghost or some other thing, either we it's not a network
            // object for our purposes
            if (___m_nview == null || ___m_nview.GetZDO()?.m_uid == null ||
                // if we got no owning piece then there's also something wrong and
                // most likely this is something we don't want to track
                // currently known "Piece"less objects: tombstone
                !TryGetValidOwningPiece(___m_nview, out var piece))
            {
                Plugin.Log.LogDebug($"Will not track container {__instance.m_name} ({__instance.name}). ZNetView: {___m_nview.GetZDO()?.m_uid}.");
                return;
            }
            ContainerTracker.Add(__instance, ___m_nview, piece);
        }
        private static bool TryGetValidOwningPiece(ZNetView zNetView, out Piece piece)
        {
            piece = zNetView.GetComponent<Piece>();
            return piece != null;
        }

        [HarmonyPatch("OnDestroyed")]
        [HarmonyPostfix]
        private static void OnDestroyed(Container __instance)
        {
            Plugin.Log.LogDebug($"{__instance?.gameObject.name} is destroyed");
            ContainerTracker.Remove(__instance);
        }
    }
}
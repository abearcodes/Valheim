﻿using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Tracking
{
    [HarmonyPatch(typeof(Player))]
    public static class PlayerLifecyclePatches
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix()]
        static void Awake(Player __instance)
        {
            Plugin.Log.LogDebug($"Tracking player {__instance.GetPlayerID()} - {__instance.GetInventory().GetHashCode()}");
            ContainerTracker.PlayerByInventoryDict[__instance.GetInventory().GetHashCode()] = __instance;
        }
        
        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix()]
        static void OnDestroy(Player __instance)
        {
            Plugin.Log.LogDebug($"Removing player {__instance.GetPlayerID()} - {__instance.GetInventory().GetHashCode()}");
            ContainerTracker.PlayerByInventoryDict[__instance.GetInventory().GetHashCode()] = __instance;
        }
        
    }
}
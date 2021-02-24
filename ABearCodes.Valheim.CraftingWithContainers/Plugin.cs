using System;
using System.Collections.Generic;
using System.Reflection;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Object = System.Object;

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
        private bool loading = false;
        public static PluginSettings Settings { get; private set; }

        private void Awake()
        {
            Settings = new PluginSettings(Config);
            var harmony = new Harmony("ABearCodes.Valheim.CraftingWithContainers");
            harmony.PatchAll();
            foreach (var patchedMethod in harmony.GetPatchedMethods())
            {
                Debug.Log(patchedMethod);
            }
        }

        private void LateUpdate()
        {
            if (loading) return;
            Debug.Log("1");
            if (FejdStartup.instance == null) return;
            Debug.Log("2");
            var m_profiles = (List<PlayerProfile>) typeof(FejdStartup)
                .GetField("m_profiles", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(FejdStartup.instance);
            Debug.Log("3");
            if (m_profiles == null) return;
            Debug.Log("4");
            
            Debug.Log("5");

            var invoke = (typeof(FejdStartup).GetMethod("SetSelectedProfile", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(FejdStartup.instance, new[] {"Dev"}));
            Debug.Log("6");
            FejdStartup.instance.OnSelectWorldTab();
            Debug.Log("7");
            
            var world = (World)(typeof(FejdStartup).GetMethod("FindWorld", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(FejdStartup.instance, new[] {"EMPTY"}));
            
            var m_world = typeof(FejdStartup)
                .GetField("m_world", BindingFlags.Instance | BindingFlags.NonPublic);
            m_world.SetValue(FejdStartup.instance, world);
            FejdStartup.instance.OnWorldStart();
            if (FejdStartup.instance.m_loading)
            {
                loading = true;
                return;
            }
            
        }

        private void OnGUI()
        {
            if (!Settings.DebugViableContainerIndicatorEnabled.Value) return;
            foreach (var containerEntry in ContainerTracker.GetViableContainersInRangeForPlayer(Player.m_localPlayer, Settings.ContainerLookupRange.Value))
            {
                var position = Camera.main.WorldToScreenPoint(containerEntry.Container.transform.position);
                GUI.color = Color.magenta;
                if (position.z > 0.01f)
                {
                    GUI.Label(
                        new Rect((float) (position.x - 50.0), Screen.height - position.y, 250f, 50f), 
                        "++");
                }
            }
        }
    }
}
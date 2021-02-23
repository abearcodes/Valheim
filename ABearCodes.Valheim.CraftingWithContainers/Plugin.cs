using System;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Patches;
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
        
        private void Awake()
        {
            Settings.BindConfig(Config);
            var harmony = new Harmony("ABearCodes.Valheim.CraftingWithContainers");
            harmony.PatchAll();
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                
                foreach (var containerEntry in Tracking.Tracker.AllContainers.Where(c => c != null))
                {
                    containerEntry.Container.GetInventory().RemoveItem("$item_wood", 1);
                    containerEntry.Container.Save();
                    containerEntry.Container.GetInventory().Changed();
                }
            }
        }

        private void OnGUI()
        {
            foreach (var containerEntry in Tracking.Tracker.AllContainers.Where(c => c != null))
            {
                var pos = Camera.main.WorldToScreenPoint(containerEntry.Container.transform.position);
                
                GUI.color = Color.green;
                if (pos.z > 0.01f)
                {
                    GUI.Label(
                        new Rect((float) (pos.x - 50.0), Screen.height - pos.y, 250f, 50f),
                        $"O:{containerEntry.ContainerCraftingNetworkExtension._zNetView.GetZDO().m_owner} | me? {containerEntry.Container.IsOwner()}");
                }
            }
        }
    }
}
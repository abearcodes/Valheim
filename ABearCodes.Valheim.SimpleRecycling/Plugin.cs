using ABearCodes.Valheim.SimpleRecycling.Recycling;
using ABearCodes.Valheim.SimpleRecycling.UI;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.SimpleRecycling
{
    [BepInPlugin("com.github.abearcodes.valheim.simplerecycling",
        "SimpleRecycling",
        "0.0.12")]
    public class Plugin : BaseUnityPlugin
    {
        public static PluginSettings Settings;
        public static ManualLogSource Log;
        private ContainerRecyclingButtonHolder _containerRecyclingButton;
        private Harmony _harmony;
        public static StationRecyclingTabHolder RecyclingTabButtonHolder { get; private set; }

        private void Awake()
        {
            Log = Logger;
            Settings = new PluginSettings(Config);
        }

        private void Start()
        {
            _harmony = new Harmony("ABearCodes.Valheim.SimpleRecycling");
            _harmony.PatchAll();
            _containerRecyclingButton = gameObject.AddComponent<ContainerRecyclingButtonHolder>();
            _containerRecyclingButton.OnRecycleAllTriggered += ContainerRecyclingTriggered;
            RecyclingTabButtonHolder = gameObject.AddComponent<StationRecyclingTabHolder>();
        }

        private void OnDestroy()
        {
            Debug.Log("Unpatching now");
            _harmony.UnpatchSelf();
        }


        // for shortness and readability
        public static string Localize(string text)
        {
            return Localization.instance.Localize(text);
        }

        private void ContainerRecyclingTriggered()
        {
            var player = Player.m_localPlayer;
            var container = (Container) AccessTools.Field(typeof(InventoryGui), "m_currentContainer")
                .GetValue(InventoryGui.instance);
            if (container == null) return;
            Log.LogDebug($"Player {player.GetPlayerName()} triggered recycling");
            Recycler.RecycleInventoryForAllRecipes(container.GetInventory(), player);
        }
    }
}
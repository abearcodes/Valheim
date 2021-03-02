using System;
using System.Collections.Generic;
using System.Linq;
using ABearCodes.Valheim.SimpleRecycling.Recycling;
using ABearCodes.Valheim.SimpleRecycling.UI;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ABearCodes.Valheim.SimpleRecycling
{
    [BepInPlugin("com.github.abearcodes.valheim.simplerecycling",
        "SimpleRecycling",
        "0.0.5")]
    public class Plugin : BaseUnityPlugin
    {
        public static PluginSettings Settings;
        public static ManualLogSource Log;
        private ContainerRecyclingButtonHolder _containerRecyclingButton;

        private void Awake()
        {
            Log = Logger;
            Settings = new PluginSettings(Config);
            
        }

        private void Start()
        {
            _containerRecyclingButton = gameObject.AddComponent<ContainerRecyclingButtonHolder>();
            _containerRecyclingButton.OnRecycleAllTriggered +=  ContainerRecyclingTriggered;
        }

        private void ContainerRecyclingTriggered()
        {
            var player = Player.m_localPlayer;
            var container = (Container) AccessTools.Field(typeof(InventoryGui), "m_currentContainer")
                .GetValue(InventoryGui.instance);
            if (container == null) return;
            var recipes = ObjectDB.instance.m_recipes
                // some recipes are just weird
                .Where(recipe => Player.m_localPlayer.IsRecipeKnown(recipe?.m_item?.m_itemData?.m_shared?.m_name))
                .ToList();
            Log.LogDebug($"Player {player.GetPlayerName()} triggered recycling");
            Recycler.RecycleInventoryForAllRecipes(container.GetInventory(), recipes, player);
        }
    }
}
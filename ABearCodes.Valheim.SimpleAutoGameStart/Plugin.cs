using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace ABearCodes.Valheim.SimpleAutoGameStart
{
    [BepInPlugin("com.github.abearcodes.valheim.simpleautogamestart",
        "Simple Auto Game Start",
        "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private ConfigEntry<bool> Enabled { get; set; }
        private ConfigEntry<string> WorldName { get; set; }
        private ConfigEntry<string> PlayerName { get; set; }

        private void Awake()
        {
            Enabled = Config.Bind("SimpleAutoGameStart", "Enabled", true, "");
            WorldName = Config.Bind("SimpleAutoGameStart", "WorldName", "", "");
            PlayerName = Config.Bind("SimpleAutoGameStart", "PlayerName", "", "");
            InvokeRepeating(nameof(LoadIntoGame), 2f, 2f);
        }

        private void LoadIntoGame()
        {
            if (!FejdStartup.instance) return;
            Logger.LogInfo("Got instance");
            Logger.LogInfo("Refreshing worlds");
            FejdStartup.instance.OnSelectWorldTab();
            var selectedWorld = World.GetWorldList()
                .FirstOrDefault(world => string.Equals(world.m_name, WorldName.Value,
                    StringComparison.CurrentCultureIgnoreCase));
            var selectedName = PlayerProfile.GetAllPlayerProfiles()
                .Select((profile, index) => new {Profile = profile, Index = index})
                .FirstOrDefault(p =>
                    string.Equals(p.Profile.GetName(), PlayerName.Value, StringComparison.CurrentCultureIgnoreCase));
            if (selectedName == null || selectedWorld == null)
            {
                Debug.LogError(
                    $"Either world name or player name are invalid. Got player {selectedName?.Profile?.GetName()}. Got world: {selectedWorld?.m_name}");
                CancelInvoke(nameof(LoadIntoGame));
                return;
            }

            // ReSharper disable once PossibleNullReferenceException
            typeof(FejdStartup).GetField("m_world", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(FejdStartup.instance, selectedWorld);
            // ReSharper disable once PossibleNullReferenceException
            typeof(FejdStartup).GetField("m_profileIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(FejdStartup.instance, selectedName.Index);
            Logger.LogInfo("Starting world");
            FejdStartup.instance.OnWorldStart();
            if (FejdStartup.instance.m_loading) CancelInvoke(nameof(LoadIntoGame));
        }
    }
}
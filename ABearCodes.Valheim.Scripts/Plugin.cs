using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.Scripts
{
    [BepInPlugin("com.github.abearcodes.valheim.scripts", 
        "Scripts",
        "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public Plugin()
        {
            Log = Logger;
        }

        public static ManualLogSource Log { get; private set; }
        
        private void Awake()
        {
            
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                
            }
        }
    }
}
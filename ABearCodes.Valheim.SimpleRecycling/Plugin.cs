using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ABearCodes.Valheim.SimpleRecycling
{
    [BepInPlugin("com.github.abearcodes.valheim.simplerecycling",
        "SimpleRecycling",
        "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        private Button recycleButton;
        private ConfigEntry<bool> Enabled { get; set; }
        private bool _prefired = false;
        private Text _textComponent;
        private Image _imageComponent;

        private void Awake()
        {
            Enabled = Config.Bind("SimpleRecycling", "Enabled", true, "Enabled or not, that's about it for now");
            Enabled.SettingChanged += (sender, args) =>
            {
                recycleButton.enabled = !Enabled.Value;
            };
        }

        private void FixedUpdate()
        {
            if (!InventoryGui.instance.IsContainerOpen() && recycleButton != null)
            {
                SetButtonState(false);
            }
            if (!(recycleButton == null) || !(InventoryGui.instance != null))
                return;
            recycleButton = Instantiate(InventoryGui.instance.m_takeAllButton,
                InventoryGui.instance.m_takeAllButton.transform);
            recycleButton.transform.localPosition = new Vector3(0f, -45.0f, -1f);
            recycleButton.transform.SetParent(InventoryGui.instance.m_takeAllButton.transform.parent);
            recycleButton.onClick.AddListener(RecycleAll);
            _textComponent = recycleButton.GetComponentInChildren<Text>();
            _imageComponent = recycleButton.GetComponentInChildren<Image>();
            SetButtonState(false);
        }

        private void SetButtonState(bool showPrefire)
        {
            if (showPrefire)
            {
                _prefired = true;
                _textComponent.text = "Confirm!?";
                _imageComponent.color = new Color(1f, 0.5f, 0.5f);
            }
            else
            {
                _prefired = false;
                _textComponent.text = "Recycle items";
                _imageComponent.color = new Color(0.5f, 1f, 0.5f);
            }
        }

        private void RecycleAll()
        {
            if (!Player.m_localPlayer)
                return;
            if (!_prefired)
            {
                SetButtonState(true);
                return;
            }
            SetButtonState(false);
            
            var player = Player.m_localPlayer;
            var container = (Container) AccessTools.Field(typeof(InventoryGui), "m_currentContainer")
                .GetValue(InventoryGui.instance);
            if (container == null) return;
            var recipes = new List<Recipe>();
            player.GetAvailableRecipes(ref recipes);
            var cInventory = container.GetInventory();
            var itemListSnapshot = new List<ItemDrop.ItemData>(cInventory.GetAllItems());
            for (var index = 0; index < itemListSnapshot.Count; index++)
            {
                var itemData = itemListSnapshot[index];
                var recipe = recipes.Find(r => r.m_item.m_itemData.m_shared.m_name == itemData.m_shared.m_name);
                if (recipe == null) continue;
                if (recipe.m_item.m_itemData.m_shared.m_name != itemData.m_shared.m_name) continue;
                foreach (var resource in recipe.m_resources)
                {
                    var rItemData = resource.m_resItem.m_itemData;
                    var preFab = ObjectDB.instance.m_items.FirstOrDefault(item =>
                        item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name == rItemData.m_shared.m_name);
                    if (preFab == null) break;
                    cInventory.AddItem(
                        preFab.name, resource.m_amount, rItemData.m_quality,
                        rItemData.m_variant, player.GetPlayerID(), player.GetPlayerName()
                    );    
                }
                cInventory.RemoveOneItem(itemData);

            }

        }
    }
}
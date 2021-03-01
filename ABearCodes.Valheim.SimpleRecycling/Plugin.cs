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
        "0.0.3")]
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
            if (InventoryGui.instance != null && recycleButton != null && !InventoryGui.instance.IsContainerOpen())
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
            var recipes = ObjectDB.instance.m_recipes
                // some recipes are just weird
                .Where(recipe => Player.m_localPlayer.IsRecipeKnown(recipe?.m_item?.m_itemData?.m_shared?.m_name))
                .ToList();
            
            var cInventory = container.GetInventory();
            var itemListSnapshot = new List<ItemDrop.ItemData>();
            itemListSnapshot.AddRange(cInventory.GetAllItems());
            
            for (var index = 0; index < itemListSnapshot.Count; index++)
            {
                var itemData = itemListSnapshot[index];
                var recyclingList = new List<RecyclingEntry>();
                var enumerable = recipes.ToList();
                var recipe = enumerable.FirstOrDefault(r => r.m_item.m_itemData.m_shared.m_name == itemData.m_shared.m_name);
                if (recipe == null) continue;
                if (recipe.m_item.m_itemData.m_shared.m_name != itemData.m_shared.m_name) continue;
                foreach (var resource in recipe.m_resources)
                {
                    var rItemData = resource.m_resItem.m_itemData;
                    var preFab = ObjectDB.instance.m_items.FirstOrDefault(item =>
                        item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name == rItemData.m_shared.m_name);
                    if (preFab == null) break;
                    var amount = resource.m_amount;
                    if (itemData.m_quality > 0)
                    {
                        amount = Enumerable.Range(1, itemData.m_quality)
                            .Select(level => resource.GetAmount(level))
                            .Sum();
                    }
                    recyclingList.Add(new RecyclingEntry(preFab, amount, rItemData.m_quality, rItemData.m_variant));
                }
                var emptySlotsAmount = cInventory.GetEmptySlots();
                var needsSlots = recyclingList.Sum(entry =>
                    Math.Ceiling(((double) entry.amount) /
                                 ((double) entry.prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxStackSize)));
                if (emptySlotsAmount < needsSlots)
                {
                    Debug.Log($"Not enough slots to recycle {itemData.m_shared.m_name}");
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, 
                        $"Could not recycle {Localization.instance.Localize(itemData.m_shared.m_name)}.\n" +
                            $"Need {needsSlots} slots but only {emptySlotsAmount} were available");
                    continue;
                }

                foreach (var entry in recyclingList)
                {
                    cInventory.AddItem(
                        entry.prefab.name, entry.amount, entry.mQuality,
                        entry.mVariant, player.GetPlayerID(), player.GetPlayerName()
                    );
                }
                cInventory.RemoveOneItem(itemData);
            }
        }
    }

    internal struct RecyclingEntry
    {
        public readonly GameObject prefab;
        public readonly int amount;
        public readonly int mQuality;
        public readonly int mVariant;

        public RecyclingEntry(GameObject prefab, int amount, int mQuality, int mVariant)
        {
            this.prefab = prefab;
            this.amount = amount;
            this.mQuality = mQuality;
            this.mVariant = mVariant;
        }
    }
}
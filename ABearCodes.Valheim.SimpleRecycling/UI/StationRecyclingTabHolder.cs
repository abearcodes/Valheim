using System;
using System.Collections.Generic;
using System.Linq;
using ABearCodes.Valheim.SimpleRecycling.Recycling;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ABearCodes.Valheim.SimpleRecycling.UI
{
    public class StationRecyclingTabHolder : MonoBehaviour
    {
        private GameObject _recyclingTabButtonGameObject;
        private Button _recyclingTabButtonComponent;
        private List<RecyclingAnalysisContext> _recyclingAnalysisContexts = new List<RecyclingAnalysisContext>();

        private void Start()
        {
            InvokeRepeating(nameof(EnsureRecyclingTabExists), 0f, 5f);
        }

        void EnsureRecyclingTabExists()
        {
            if (InventoryGui.instance == null) return;
            if (_recyclingTabButtonComponent == null) SetupTabButton();
            if (_recyclingTabButtonGameObject.activeSelf != Plugin.Settings.EnableExperimentalCraftingTabUI.Value)
            {
                _recyclingTabButtonGameObject.SetActive(Plugin.Settings.EnableExperimentalCraftingTabUI.Value);
            }
        }

        private void OnDestroy()
        {
            Debug.Log("Destroying... 1");
            Destroy(_recyclingTabButtonGameObject.gameObject);
        }

        private void SetupTabButton()
        {
            Plugin.Log.LogDebug("Creating tab button");
            var upgradeTabTransform = InventoryGui.instance.m_tabUpgrade.transform;
            _recyclingTabButtonGameObject = Instantiate(InventoryGui.instance.m_tabUpgrade.gameObject,
                upgradeTabTransform.position, upgradeTabTransform.rotation, upgradeTabTransform.parent);

            _recyclingTabButtonGameObject.name = "RECYCLE";
            // Unity3d is inconsistent and for whatever reason game object order in the parent transform
            // matters for the UI components 😐
            _recyclingTabButtonGameObject.transform.parent.Find("TabBorder").SetAsLastSibling();
            _recyclingTabButtonGameObject.transform.localPosition = new Vector3(-70, -94, 0);

            var textComponent = _recyclingTabButtonGameObject.GetComponentInChildren<Text>();
            textComponent.text = "RECYCLE";
            var imageComponent = _recyclingTabButtonGameObject.GetComponent<Image>();
            // imageComponent.color = new Color(0.7f, 1f, 0.7f);

            _recyclingTabButtonComponent = _recyclingTabButtonGameObject.GetComponent<Button>();
            _recyclingTabButtonComponent.interactable = true;
            _recyclingTabButtonComponent.onClick.RemoveAllListeners();
            _recyclingTabButtonComponent.onClick.AddListener(OnRecycleClick);

            // _recyclingTabButton.transform.parent = InventoryGui.instance.m_tabUpgrade.transform.parent;

            // _recyclingTabButton.onClick.AddListener(OnRecycleAllPressed);
            // _textComponent = _recyclingTabButton.GetComponentInChildren<Text>();
            // _imageComponent = _recyclingTabButton.GetComponentInChildren<Image>();
            // var dragger = _recyclingTabButton.gameObject.AddComponent<UIDragger>();
            // dragger.OnUIDropped += (source, position) =>
            // {
            //     Plugin.Settings.ContainerRecyclingButtonPositionJsonString.Value = JsonUtility.ToJson(position);
            // };
            // SetButtonState(false);
        }

        private void OnRecycleClick()
        {
            _recyclingTabButtonComponent.interactable = false;
            InventoryGui.instance.m_tabCraft.interactable = true;
            InventoryGui.instance.m_tabUpgrade.interactable = true;
            Plugin.Log.LogDebug($"OnRecycleClick");
            UpdateCraftingPanel();
        }

        public void UpdateCraftingPanel()
        {
            var igui = InventoryGui.instance;
            var localPlayer = Player.m_localPlayer;
            if (!(bool) localPlayer.GetCurrentCraftingStation() && !localPlayer.NoCostCheat())
            {
                igui.m_tabCraft.interactable = false;
                igui.m_tabUpgrade.interactable = true;
                igui.m_tabUpgrade.gameObject.SetActive(false);

                _recyclingTabButtonComponent.interactable = true;
                _recyclingTabButtonComponent.gameObject.SetActive(false);
            }
            else
                igui.m_tabUpgrade.gameObject.SetActive(true);

            UpdateRecyclingList();


            if (igui.get_m_availableRecipes().Count > 0)
            {
                if (igui.get_m_selectedRecipe().Key != null)
                    igui.SetRecipe(igui.GetSelectedRecipeIndex(), true);
                else
                    igui.SetRecipe(0, true);
            }
            else
                igui.SetRecipe(-1, true);
        }

        private void UpdateRecyclingList()
        {
            var localPlayer = Player.m_localPlayer;
            var igui = InventoryGui.instance;
            igui.get_m_availableRecipes().Clear();
            var m_recipeList = igui.get_m_recipeList();
            Plugin.Log.LogDebug($"Old recipe list had {m_recipeList.Count} entries. Cleaning up");
            foreach (var recipeElement in m_recipeList) Destroy(recipeElement);
            m_recipeList.Clear();

            _recyclingAnalysisContexts.Clear();
            var validRecycles = Recycler.GetRecyclingAnalysisForInventory(localPlayer.GetInventory(), localPlayer)
                .Where(context => context.Recipe != null);
            _recyclingAnalysisContexts.AddRange(validRecycles);
            foreach (var context in _recyclingAnalysisContexts)
            {
                if (context.Recipe == null) continue;
                AddRecipeToList(context, m_recipeList);
            }

            Plugin.Log.LogDebug($"Added {m_recipeList.Count} entries");

            igui.m_recipeListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                Mathf.Max(igui.get_m_recipeListBaseSize(), m_recipeList.Count * igui.m_recipeListSpace));
        }

        private void AddRecipeToList(RecyclingAnalysisContext context, List<GameObject> m_recipeList)
        {
            var count = m_recipeList.Count;

            var igui = InventoryGui.instance;
            var m_recipeListRoot = igui.m_recipeListRoot;
            var element = Instantiate(igui.m_recipeElementPrefab, m_recipeListRoot);
            element.SetActive(true);
            ((RectTransform) element.transform).anchoredPosition = new Vector2(0.0f, count * -igui.m_recipeListSpace);
            var component1 = element.transform.Find("icon").GetComponent<Image>();
            component1.sprite = context.Item.GetIcon();
            component1.color = context.Impediments.Count == 0 ? Color.white : new Color(1f, 0.0f, 1f, 0.0f);
            var component2 = element.transform.Find("name").GetComponent<Text>();
            var str = Localization.instance.Localize(context.Item.m_shared.m_name);
            if (context.Item.m_stack > 1 && context.Item.m_shared.m_maxStackSize > 1)
                str = str + " x" + context.Item.m_stack;
            component2.text = str;
            component2.color = context.Impediments.Count == 0 ? Color.white : new Color(0.66f, 0.66f, 0.66f, 1f);
            var component3 = element.transform.Find("Durability").GetComponent<GuiBar>();
            if (context.Item.m_shared.m_useDurability &&
                context.Item.m_durability < (double) context.Item.GetMaxDurability())
            {
                component3.gameObject.SetActive(true);
                component3.SetValue(context.Item.GetDurabilityPercentage());
            }
            else
                component3.gameObject.SetActive(false);

            var component4 = element.transform.Find("QualityLevel").GetComponent<Text>();

            component4.gameObject.SetActive(true);
            component4.text = context.Item.m_quality.ToString();

            element.GetComponent<Button>().onClick.AddListener(() => igui.OnSelectedRecipe(element));
            m_recipeList.Add(element);
            igui.get_m_availableRecipes()
                .Add(new KeyValuePair<Recipe, ItemDrop.ItemData>(context.Recipe, context.Item));
        }


        public bool InRecycleTab() => !_recyclingTabButtonComponent.interactable;

        public void SetInteractable(bool interactable)
        {
            _recyclingTabButtonComponent.interactable = interactable;
        }

        public void SetActive(bool active)
        {
            if (!Plugin.Settings.EnableExperimentalCraftingTabUI.Value) return;
            
            _recyclingTabButtonGameObject.SetActive(active);
        }

        public void UpdateRecipe(Player player, float dt)
        {
            //todo: optimize reflection calls
            var igui = InventoryGui.instance;
            
            var selectedRecipeIndex = igui.GetSelectedRecipeIndex();
            if (selectedRecipeIndex != -1 && selectedRecipeIndex < _recyclingAnalysisContexts.Count)
            {
                var context = _recyclingAnalysisContexts[selectedRecipeIndex];
                var newContext = new RecyclingAnalysisContext(context.Item);
                Recycler.TryAnalyzeOneItem(newContext, player.GetInventory(), player);
                _recyclingAnalysisContexts[selectedRecipeIndex] = newContext;
            }

            var currentCraftingStation = player.GetCurrentCraftingStation();
            if ((bool) currentCraftingStation)
            {
                igui.m_craftingStationName.text = Localization.instance.Localize(currentCraftingStation.m_name);
                igui.m_craftingStationIcon.gameObject.SetActive(true);
                igui.m_craftingStationIcon.sprite = currentCraftingStation.m_icon;
                igui.m_craftingStationLevel.text = currentCraftingStation.GetLevel().ToString();
                igui.m_craftingStationLevelRoot.gameObject.SetActive(true);
            }
            else
            {
                igui.m_craftingStationName.text = Localization.instance.Localize("$hud_crafting");
                igui.m_craftingStationIcon.gameObject.SetActive(false);
                igui.m_craftingStationLevelRoot.gameObject.SetActive(false);
            }

            if (igui.get_m_selectedRecipe().Key)
            {
                var analysisContext = _recyclingAnalysisContexts[igui.GetSelectedRecipeIndex()];
                igui.m_recipeIcon.enabled = true;
                igui.m_recipeName.enabled = true;
                igui.m_recipeDecription.enabled = true;
                ItemDrop.ItemData itemData = igui.get_m_selectedRecipe().Value;
                int num = itemData != null ? itemData.m_quality + 1 : 1;
                bool allowedQuality = num <= igui.get_m_selectedRecipe().Key.m_item.m_itemData.m_shared.m_maxQuality;
                igui.m_recipeIcon.sprite = igui.get_m_selectedRecipe().Key.m_item.m_itemData.m_shared
                    .m_icons[itemData != null ? itemData.m_variant : igui.get_m_selectedVariant()];
                string str =
                    Localization.instance.Localize(igui.get_m_selectedRecipe().Key.m_item.m_itemData.m_shared.m_name);
                if (analysisContext.Item.m_stack > 1)
                    str = str + " x" + analysisContext.Item.m_stack;
                igui.m_recipeName.text = str;
                igui.m_recipeDecription.text = Localization.instance.Localize(
                ItemDrop.ItemData.GetTooltip(igui.get_m_selectedRecipe().Key.m_item.m_itemData, num, true));
                if (analysisContext.Impediments.Count == 0)
                    igui.m_recipeDecription.text = "\nAll requirements are <color=orange>fulfilled</color>";
                else
                    igui.m_recipeDecription.text = $"\nRecycling blocked by these reasons:\n\n<size=10>" +
                                                   $"{string.Join("\n", analysisContext.Impediments)}" +
                                                   $"</size>";
                if (itemData != null)
                {
                    igui.m_itemCraftType.gameObject.SetActive(true);
                    igui.m_itemCraftType.text =
                        Localization.instance.Localize(
                            $"Recycle {itemData.m_shared.m_name} of quality {itemData.m_quality}");
                }
                else
                    igui.m_itemCraftType.gameObject.SetActive(false);

                igui.m_variantButton.gameObject.SetActive(
                    igui.get_m_selectedRecipe().Key.m_item.m_itemData.m_shared.m_variants > 1 &&
                    igui.get_m_selectedRecipe().Value == null);
                SetupRequirementList(analysisContext);
                
                igui.m_minStationLevelIcon.gameObject.SetActive(false);
                igui.m_craftButton.interactable = analysisContext.Impediments.Count == 0;
                igui.m_craftButton.GetComponentInChildren<Text>().text = "Recycle";
                if(analysisContext.Impediments.Count == 0)
                    igui.m_craftButton.GetComponent<UITooltip>().m_text = "";
                else
                    igui.m_craftButton.GetComponent<UITooltip>().m_text = Localization.instance.Localize("$msg_missingrequirement");
                
            }
            else
            {
                igui.m_recipeIcon.enabled = false;
                igui.m_recipeName.enabled = false;
                igui.m_recipeDecription.enabled = false;
                igui.m_qualityPanel.gameObject.SetActive(false);
                igui.m_minStationLevelIcon.gameObject.SetActive(false);
                igui.m_craftButton.GetComponent<UITooltip>().m_text = "";
                igui.m_variantButton.gameObject.SetActive(false);
                igui.m_itemCraftType.gameObject.SetActive(false);
                // for (int index = 0; index < igui.m_recipeRequirementList.Length; ++index)
                //     InventoryGui.HideRequirement(igui.m_recipeRequirementList[index].transform);
                igui.m_craftButton.interactable = false;
            }

            if (igui.get_m_craftTimer() < 0.0)
            {
              igui.m_craftProgressPanel.gameObject.SetActive(false);
              igui.m_craftButton.gameObject.SetActive(true);
            }
            else
            {
              igui.m_craftButton.gameObject.SetActive(false);
              igui.m_craftProgressPanel.gameObject.SetActive(true);
              igui.m_craftProgressBar.SetMaxValue(igui.m_craftDuration);
              igui.m_craftProgressBar.SetValue(igui.get_m_craftTimer());
              igui.set_m_craftTimer(igui.get_m_craftTimer() + dt);
              if (igui.get_m_craftTimer() < (double) igui.m_craftDuration)
                return;
              Recycler.DoInventoryChanges(_recyclingAnalysisContexts[selectedRecipeIndex], player.GetInventory(), player);
              igui.set_m_craftTimer(-1f);
              igui.SetRecipe(-1, false);
              UpdateCraftingPanel();
              
            }
        }

        private void SetupRequirementList(RecyclingAnalysisContext analysisContexts)
        {
            var igui = InventoryGui.instance;
            
            for (int i = 0; i < igui.m_recipeRequirementList.Length; i++)
            {
                var elementTransform = igui.m_recipeRequirementList[i].transform;
                if (i < analysisContexts.Entries.Count)
                {
                    var entry = analysisContexts.Entries[i];
                    if(entry.Amount == 0) 
                        InventoryGui.HideRequirement(elementTransform);
                    else
                        SetupRequirement(elementTransform, entry);
                }
                else
                {
                    InventoryGui.HideRequirement(elementTransform);
                    continue;
                }
            }

        }

        public static void SetupRequirement(Transform elementRoot,
            RecyclingAnalysisContext.RecyclingYieldEntry entry)
        {
            Image component1 = elementRoot.transform.Find("res_icon").GetComponent<Image>();
            Text component2 = elementRoot.transform.Find("res_name").GetComponent<Text>();
            Text component3 = elementRoot.transform.Find("res_amount").GetComponent<Text>();
            UITooltip component4 = elementRoot.GetComponent<UITooltip>();
            component1.gameObject.SetActive(true);
            component2.gameObject.SetActive(true);
            component3.gameObject.SetActive(true);
            component1.sprite = entry.RecipeItemData.GetIcon();
            component1.color = Color.white;
            component4.m_text = Localization.instance.Localize(entry.RecipeItemData.m_shared.m_name);
            component2.text = Localization.instance.Localize(entry.RecipeItemData.m_shared.m_name);
            component3.text = entry.Amount.ToString();
            component3.color = Color.white;
        }
    }
}
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.SimpleRecycling.UI
{
    public static class InventoryGuiExtensions
    {
        public static float get_m_recipeListBaseSize(this InventoryGui instance)
        {
            return (float) AccessTools.Field(typeof(InventoryGui), "m_recipeListBaseSize").GetValue(instance);
        }
        public static List<GameObject> get_m_recipeList(this InventoryGui instance)
        {
            return (List<GameObject>)AccessTools.Field(typeof(InventoryGui), "m_recipeList").GetValue(instance);
        }

        public static List<KeyValuePair<Recipe, ItemDrop.ItemData>> get_m_availableRecipes(this InventoryGui instance)
        {
            return (List<KeyValuePair<Recipe, ItemDrop.ItemData>>) AccessTools.Field(typeof(InventoryGui),
                "m_availableRecipes").GetValue(instance);
        }

        public static KeyValuePair<Recipe, ItemDrop.ItemData> get_m_selectedRecipe(this InventoryGui instance)
        {
            return (KeyValuePair<Recipe, ItemDrop.ItemData>) AccessTools.Field(typeof(InventoryGui), "m_selectedRecipe")
                .GetValue(instance);
        }
        public static void set_m_selectedRecipe(this InventoryGui instance, KeyValuePair<Recipe, ItemDrop.ItemData> value)
        {
           AccessTools.Field(typeof(InventoryGui), "m_selectedRecipe")
                .SetValue(instance, value);
        }

        public static int get_m_selectedVariant(this InventoryGui instance)
        {
            return (int) AccessTools.Field(typeof(InventoryGui), "m_selectedVariant").GetValue(instance);
        }
        
        public static void set_m_selectedVariant(this InventoryGui instance, int value)
        {
            AccessTools.Field(typeof(InventoryGui), "m_selectedVariant")
                .SetValue(instance, value);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ABearCodes.Valheim.SimpleRecycling.Recycling
{
    internal struct RecyclingAnalysisContext
    {
        public ItemDrop.ItemData Item;
        public List<string> Impediments { get; }
        public List<RecyclingYieldEntry> Entries { get; }

        public bool ShouldErrorDumpAnalysis { get; set; }

        public RecyclingAnalysisContext(ItemDrop.ItemData item) : this()
        {
            Item = item;
            Impediments = new List<string>();
            Entries = new List<RecyclingYieldEntry>();
            ShouldErrorDumpAnalysis = false;
        }

        internal struct RecyclingYieldEntry
        {
            public readonly GameObject Prefab;
            public readonly Recipe Recipe;
            public readonly int Amount;
            public readonly int mQuality;
            public readonly int mVariant;
            public readonly bool InitialRecipeHadZero;

            public RecyclingYieldEntry(GameObject prefab, Recipe recipe, int amount, int mQuality, int mVariant, bool initialRecipeHadZero)
            {
                this.Prefab = prefab;
                this.Recipe = recipe;
                this.Amount = amount;
                this.mQuality = mQuality;
                this.mVariant = mVariant;
                this.InitialRecipeHadZero = initialRecipeHadZero;
            }
        }

        public void Dump()
        {
            var dumpObject = new
            {
                ItemName = Item.m_shared.m_name,
                ItemQuality = Item.m_quality,
                ItemStacks = Item.m_stack,
                ItemMaxStacks = Item.m_shared.m_maxStackSize,
                Impediments = Impediments,
                Recipe = Entries.Take(1).Select(entry =>
                {
                    var prefabItemData = entry.Prefab.GetComponent<ItemDrop>()?.m_itemData;
                    if (prefabItemData == null)
                    {
                        Plugin.Log.LogError($"Could not get item data on prefab {entry.Prefab.name}");
                        return null;
                    }
                    return new
                    {
                        ItemName = prefabItemData.m_shared.m_name,
                        RecipeName = entry.Recipe.name,
                        RecipeCraftedAmount = entry.Recipe.m_amount,
                        Resources = entry.Recipe.m_resources.Select(resource => new
                        {
                            Name = resource.m_resItem.m_itemData.m_shared.m_name,
                            Amount = resource.m_amount,
                            AmountPerLevel = resource.m_amountPerLevel,
                            Quality = resource.m_resItem.m_itemData.m_quality,
                            Variant = resource.m_resItem.m_itemData.m_variant,
                        }).ToList()
                    };
                }).Where(entry => entry != null).ToList(),
                Entries = Entries.Select(entry => new
                {
                    PrefabName = entry.Prefab.name,
                    Amount = entry.Amount,
                    Quality = entry.mQuality,
                    Variant = entry.mVariant,
                }).ToList()
            };
            var sb = new StringBuilder();
            sb.AppendLine("\n==== Dump of recycling analysis was requested ====");
            sb.AppendLine(ObjectDumper.Dump(dumpObject));
            sb.AppendLine("==== Dump ends here ====");
            Plugin.Log.LogError(sb.ToString());
        }
    }
}
using System.Collections.Generic;
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
            public readonly GameObject prefab;
            public readonly int amount;
            public readonly int mQuality;
            public readonly int mVariant;

            public RecyclingYieldEntry(GameObject prefab, int amount, int mQuality, int mVariant)
            {
                this.prefab = prefab;
                this.amount = amount;
                this.mQuality = mQuality;
                this.mVariant = mVariant;
            }
        }
    }
}
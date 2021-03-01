using UnityEngine;

namespace ABearCodes.Valheim.SimpleRecycling.Recycling
{
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
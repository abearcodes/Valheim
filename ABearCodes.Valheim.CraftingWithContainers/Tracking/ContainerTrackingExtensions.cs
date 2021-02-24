using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Tracking
{
    public static class ContainerTrackingExtensions
    {
        public static Dictionary<long, Player> PlayerByInventoryDict { get; } = new Dictionary<long, Player>();

        private static readonly List<TrackedContainer> _containers = new List<TrackedContainer>();

        private static readonly Dictionary<long, ContainerNetworkExtension> _extensions =
            new Dictionary<long, ContainerNetworkExtension>();

        private static readonly Dictionary<long, CachedEntry> _containersInRangeOfPlayerCache =
            new Dictionary<long, CachedEntry>();
        

        public static void Add(Container container, ZNetView zNetView, Piece piece)
        {
            _containers.Add(new TrackedContainer(container, zNetView, piece));
            // var extension = new ContainerNetworkExtension(container, zNetView);
            // extension.Register();
            // _extensions[container.GetInstanceID()] = extension;
        }

        public static void Remove(Container container)
        {
            _containers.RemoveAll(tracked => tracked.Container);
            // if (!_extensions.TryGetValue(container.GetInstanceID(), out var extension))
            //     return;
            // extension.Unregister();
            // _extensions.Remove(container.GetInstanceID());
        }

        public static List<TrackedContainer> GetViableContainersInRangeForPlayer(Player player, float range)
        {
            var playerID = player.GetPlayerID();
            if (TryGetFromCache(playerID, out var cachedContainers))
                return cachedContainers;
            Plugin.Log.LogDebug($"No cache hit for {playerID}. Searching for new ");
            var newFoundContainers = ContainerSearcher.SearchForViablePlayerContainers(_containers, player, range);
            _containersInRangeOfPlayerCache[playerID] = new CachedEntry(Time.time, newFoundContainers);
            return newFoundContainers;
        }

        private static bool TryGetFromCache(long playerID, out List<TrackedContainer> containers)
        {
            if (_containersInRangeOfPlayerCache.TryGetValue(playerID, out var lastCachedItem))
            {
                if (Time.time - lastCachedItem.CachedAt < 2)
                {
                    if (_containersInRangeOfPlayerCache.TryGetValue(playerID, out CachedEntry containersInRange))
                    {
                        containers = containersInRange.Item;
                        return true;
                    }
                }
            }
            containers = null;
            return false;
        }

        private struct CachedEntry
        {
            public CachedEntry(float cachedAt, List<TrackedContainer> item)
            {
                CachedAt = cachedAt;
                Item = item;
            }

            public float CachedAt { get; }
            public List<TrackedContainer> Item { get; }
        }
    }

    public struct TrackedContainer
    {
        public TrackedContainer(Container container, ZNetView zNetView, Piece owningPiece)
        {
            Container = container;
            ZNetView = zNetView;
            OwningPiece = owningPiece;
        }

        public Container Container { get; }
        public ZNetView ZNetView { get; }
        public Piece OwningPiece { get; }
    }
}
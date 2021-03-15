using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Tracking
{
    public static partial class ContainerTracker
    {
        private static readonly List<TrackedContainer> _containers = new List<TrackedContainer>();

        internal static readonly Dictionary<long, ContainerNetworkExtension> _networkExtensions =
            new Dictionary<long, ContainerNetworkExtension>();

        public static Dictionary<long, Player> PlayerByInventoryDict { get; } = new Dictionary<long, Player>();

        public static void Add(Container container, ZNetView zNetView, Piece piece)
        {
            _containers.RemoveAll(tracked => tracked.Container == null);
            _containers.Add(new TrackedContainer(container, zNetView, piece));
            var extension = new ContainerNetworkExtension(container, zNetView);
            extension.Register();
            _networkExtensions[container.GetInstanceID()] = extension;
        }

        public static void Remove(Container container)
        {
            _containers.RemoveAll(tracked => tracked.Container == null || tracked.Container == container);
            if (!_networkExtensions.TryGetValue(container.GetInstanceID(), out var extension))
                return;
            extension.Unregister();
            _networkExtensions.Remove(container.GetInstanceID());
        }

        public static List<TrackedContainer> GetViableContainersInRangeForPlayer(Player player, float range)
        {
            return ContainerSearcher.SearchForViablePlayerContainers(_containers, player, range);
        }

        /// <summary>
        ///     Only used when hot-reloading scripts
        /// </summary>
        public static void ForceScanContainers()
        {
            var containers = Object.FindObjectsOfType<Container>();
            Plugin.Log.LogDebug($"Found a total of {containers.Length} containers");
            foreach (var container in containers)
            {
                var zNetView = (ZNetView) AccessTools.Field(typeof(Container), "m_nview").GetValue(container);
                var piece = zNetView.GetComponent<Piece>();
                if (zNetView == null || zNetView.GetZDO()?.m_uid == null ||
                    piece == null)
                {
                    Plugin.Log.LogDebug(
                        $"Will not track container {container.m_name} ({container.name}). Inventory hash: {container.GetInventory().GetHashCode()} ZNetView: {zNetView.GetZDO()?.m_uid}.");
                    continue;
                }

                Add(container, zNetView, piece);
            }

            Plugin.Log.LogDebug($"Added {_containers.Count} containers");

            foreach (var player in Player.GetAllPlayers())
                PlayerByInventoryDict[player.GetInventory().GetHashCode()] = player;
            Plugin.Log.LogDebug($"Added {PlayerByInventoryDict.Count} players");
        }
    }

    public readonly struct TrackedContainer
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

        public ContainerNetworkExtension NetworkExtension => ContainerTracker._networkExtensions[Container.GetInstanceID()];
    }
}
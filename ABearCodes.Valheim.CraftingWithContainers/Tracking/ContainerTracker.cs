using System.Collections.Generic;

namespace ABearCodes.Valheim.CraftingWithContainers.Tracking
{
    public static partial class ContainerTracker
    {
        public static Dictionary<long, Player> PlayerByInventoryDict { get; } = new Dictionary<long, Player>();

        private static readonly List<TrackedContainer> _containers = new List<TrackedContainer>();

        private static readonly Dictionary<long, ContainerNetworkExtension> _extensions =
            new Dictionary<long, ContainerNetworkExtension>();
        
        public static void Add(Container container, ZNetView zNetView, Piece piece)
        {
            _containers.RemoveAll(tracked => tracked.Container == null);
            _containers.Add(new TrackedContainer(container, zNetView, piece));
            // var extension = new ContainerNetworkExtension(container, zNetView);
            // extension.Register();
            // _extensions[container.GetInstanceID()] = extension;
        }

        public static void Remove(Container container)
        {
            _containers.RemoveAll(tracked => tracked.Container == null || tracked.Container == container);
            // if (!_extensions.TryGetValue(container.GetInstanceID(), out var extension))
            //     return;
            // extension.Unregister();
            // _extensions.Remove(container.GetInstanceID());
        }

        public static List<TrackedContainer> GetViableContainersInRangeForPlayer(Player player, float range) => 
            ContainerSearcher.SearchForViablePlayerContainers(_containers, player, range);
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
    }
}
using System.Collections.Generic;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Patches;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Tracking
{
    public static class ContainerSearcher
    {
        private static bool IsContainerOnAllowedPiece(Piece owningPiece)
        {
            return !Plugin.Settings.ShouldFilterByContainerPieceNames.Value ||
                   Plugin.Settings.AllowedContainerLookupPieceNamesAsList
                       .Contains(owningPiece.m_name);
        }

        private static bool IsContainerOwningPiecePlacedByPlayer(Piece piece)
        {
            return piece.GetCreator() != 0;
        }

        private static bool IsContainerInRange(Container container, Vector3 position, float range)
        {
            return Vector3.Distance(container.transform.position, position) < range;
        }


        public static List<TrackedContainer> SearchForViablePlayerContainers(List<TrackedContainer> allContainers,
            Player player, float range)
        {
            var playerID = player.GetPlayerID();
            var playerPosition = player.transform.position;
            // todo: deal with guardstones :(
            return allContainers.Where(tracked => tracked.Container != null
                                                  && tracked.Container.CheckAccess(playerID)
                                                  && IsContainerInRange(tracked.Container, playerPosition, range)
                                                  && IsContainerOnAllowedPiece(tracked.OwningPiece)
                                                  && IsContainerOwningPiecePlacedByPlayer(tracked.OwningPiece))
                .ToList();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABearCodes.Valheim.CraftingWithContainers.Common;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using ABearCodes.Valheim.CraftingWithContainers.UI;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Inventoring
{
    public static class InventoryItemRemover
    {
        public static void IterateAndRemoveItemsFromInventories(Player player, List<TrackedContainer> containers,
            string itemName, int amount, out RemovalReport report)
        {
            report = new RemovalReport(itemName, amount);
            var leftToRemove = amount;

            if (leftToRemove > 0 && Plugin.Settings.TakeFromPlayerInventoryFirst.Value)
            {
                var itemsRemoved = player.GetInventory().RemoveItemAsMuchAsPossible(itemName, leftToRemove);
                leftToRemove -= itemsRemoved;
                if (itemsRemoved > 0)
                    report.Removals.Add(new RemovalReport.RemovalReportEntry(true, null, itemsRemoved));
            }

            foreach (var tracked in containers)
            {
                var itemsRemoved = tracked.Container.GetInventory().RemoveItemAsMuchAsPossible(itemName, leftToRemove);
                leftToRemove -= itemsRemoved;
                if (itemsRemoved > 0)
                {
                    report.Removals.Add(new RemovalReport.RemovalReportEntry(false, tracked, itemsRemoved));
                    UpdateContainerNetworkData(player, tracked, itemName, amount);
                }
                
                if (leftToRemove == 0)
                    break;
            }

            if (leftToRemove > 0 && !Plugin.Settings.TakeFromPlayerInventoryFirst.Value)
            {
                var itemsRemoved = player.GetInventory().RemoveItemAsMuchAsPossible(itemName, leftToRemove);
                leftToRemove -= itemsRemoved;
                if (itemsRemoved > 0)
                    report.Removals.Add(new RemovalReport.RemovalReportEntry(true, null, itemsRemoved));
            }

            if (leftToRemove != 0 || Plugin.Settings.DebugForcePrintRemovalReport.Value)
            {
                var nearbyPlayers = new List<Character>();
                Character.GetCharactersInRange(player.transform.position, Plugin.Settings.ContainerLookupRange.Value,
                    nearbyPlayers);
                var playerCount = nearbyPlayers.Count(character => character.IsPlayer());
                Plugin.Log.LogWarning("Invalid state reached (or dump explicitly requested)! \n" +
                                      "You might want to report this to the mod developer.\n" +
                                      $"When removing {amount} of {itemName}, amount of resources left to remove was still {leftToRemove}\n" +
                                      $"Containers: {containers.Count}. Players: {playerCount}.\n" +
                                      $"{report.GetReportString()}");
            }
        }

        private static void UpdateContainerNetworkData(Player player, TrackedContainer container, string itemName, int amount)
        {
            if (container.ZNetView.IsOwner())
            {
                container.Container.Save();
                ZDOMan.instance.ForceSendZDO(player.GetPlayerID(), container.ZNetView.GetZDO().m_uid);
                return;
            }
            Plugin.Log.LogDebug($"Sending networking notification for {itemName} {amount}");
            container.NetworkExtension.RequestItemRemoval(player.GetPlayerID(), itemName, amount);
        }

        public static void RemoveFromSpecificContainer(ItemDrop.ItemData item, TrackedContainer usedContainer,
            Player player)
        {
            Plugin.Log.LogDebug(
                $"{player.GetPlayerName()} requested removal of {item.m_shared.m_name} from {usedContainer.OwningPiece.m_name}");
            usedContainer.Container.GetInventory().RemoveItem(item, 1);
            UpdateContainerNetworkData(player, usedContainer, item.m_shared.m_name, 1);
            SpawnEffect(player, usedContainer);
        }

        public static void SpawnEffect(Player player, TrackedContainer container)
        {
            Plugin.Log.LogDebug(
                $"Attaching effect between player {player.GetPlayerName()} and {container.Container.m_name}({container.ZNetView.GetZDO().m_uid})");
            LineEffectCreator.Create(container.Container.transform.position, player.transform,
                0.1f, 0.01f, 0.3f, 0.5f);
        }

        public struct RemovalReport
        {
            public RemovalReport(string itemName, int amount)
            {
                ItemName = itemName;
                Amount = amount;
                Removals = new List<RemovalReportEntry>();
            }

            public string ItemName { get; }
            public int Amount { get; }
            public List<RemovalReportEntry> Removals { get; }

            public struct RemovalReportEntry
            {
                public RemovalReportEntry(bool usedPlayerInventory, TrackedContainer? trackedContainer,
                    int amountRemoved)
                {
                    UsedPlayerInventory = usedPlayerInventory;
                    TrackedContainer = trackedContainer;
                    AmountRemoved = amountRemoved;
                }

                public bool UsedPlayerInventory { get; }
                public TrackedContainer? TrackedContainer { get; }
                public int AmountRemoved { get; }
            }

            public string GetReportString(bool colorize = false)
            {
                const string removedHeaderFormat = "Removal of {0} \"{1}\" touched {2} inventories\n";
                const string removedHeaderFormatColor =
                    "Removed <color=lightblue>{0}</color> <color=orange>\"{1}\"</color>. Touched <color=lightblue>{2}</color> inventories\n";
                const string playerEntryFormat = "Player: {0}\n";
                const string playerEntryFormatColor = "<color=cyan>Player</color>: <color=lightblue>{0}</color>\n";
                const string containerEntryFormat = "{0}: {1}\n";
                const string containerEntryFormatColor = "<color=cyan>{0}</color>: <color=lightblue>{1}</color>\n";

                var sb = new StringBuilder();
                sb.AppendFormat(colorize ? removedHeaderFormatColor : removedHeaderFormat, Amount,
                    Localization.instance.Localize(ItemName), Removals.Count);

                foreach (var removal in Removals)
                    if (removal.UsedPlayerInventory)
                        sb.AppendFormat(colorize ? playerEntryFormatColor : playerEntryFormat, removal.AmountRemoved);
                    else
                        sb.AppendFormat(colorize ? containerEntryFormatColor : containerEntryFormat,
                            Localization.instance.Localize(removal.TrackedContainer?.Container.m_name),
                            removal.AmountRemoved);

                return sb.ToString();
            }
        }
    }
}
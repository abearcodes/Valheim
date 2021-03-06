using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using ABearCodes.Valheim.CraftingWithContainers.Patches;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using ABearCodes.Valheim.CraftingWithContainers.Utils;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Crafting
{
    public static class InventoryItemRemover
    {
        public static void IterateAndRemoveItemsFromInventories(Player player, List<TrackedContainer> containers,
            string name, int amount, out RemovalReport report)
        {
            report = new RemovalReport(name, amount);
            var leftToRemove = amount;

            if (leftToRemove > 0 && Plugin.Settings.TakeFromPlayerInventoryFirst.Value)
            {
                var itemsRemoved = player.GetInventory().RemoveItemAsMuchAsPossible(name, amount);
                leftToRemove -= itemsRemoved;
                if (itemsRemoved > 0)
                    report.Removals.Add(new RemovalReport.RemovalReportEntry(true, null, itemsRemoved));
            }
            
            foreach (var container in containers)
            {
                var itemsRemoved = container.Container.GetInventory().RemoveItemAsMuchAsPossible(name, leftToRemove);
                leftToRemove -= itemsRemoved;
                if (itemsRemoved > 0)
                    report.Removals.Add(new RemovalReport.RemovalReportEntry(false, container, itemsRemoved));

                UpdateContainerNetworkData(player, container.Container);
                if (leftToRemove == 0)
                    break;
            }

            if (leftToRemove > 0 && !Plugin.Settings.TakeFromPlayerInventoryFirst.Value)
            {
                var itemsRemoved = player.GetInventory().RemoveItemAsMuchAsPossible(name, amount);
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
                Plugin.Log.LogWarning("Invalid state reached! You might want to report this to the mod developer.\n" +
                                    $"When removing {amount} of {name}, amount of resources left to remove was still {leftToRemove}\n" +
                                    $"Containers: {containers.Count}. Players: {playerCount}.\n" +
                                    $"{report.GetReportString()}");
            }
        }

        private static void UpdateContainerNetworkData(Player player, Container container)
        {
            var containerZNewView = (ZNetView) AccessTools.Field(typeof(Container), "m_nview").GetValue(container);
            container.Save();
            var containerUid = containerZNewView.GetZDO().m_uid;
            ZDOMan.instance.ForceSendZDO(player.GetPlayerID(), containerUid);
            containerZNewView.GetZDO().SetOwner(player.GetPlayerID());
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
                const string removedHeaderFormat =  "Removed {0} \"{1}\". Touched {2} inventories\n";
                const string removedHeaderFormatColor =  "Removed <color=lightblue>{0}</color> <color=orange>\"{1}\"</color>. Touched <color=lightblue>{2}</color> inventories\n";
                const string playerEntryFormat = "Player: {0}\n";
                const string playerEntryFormatColor = "<color=cyan>Player</color>: <color=lightblue>{0}</color>\n";
                const string containerEntryFormat = "{0}: {1}\n";
                const string containerEntryFormatColor = "<color=cyan>{0}</color>: <color=lightblue>{1}</color>\n";
                
                var sb = new StringBuilder();
                sb.AppendFormat(colorize ? removedHeaderFormatColor : removedHeaderFormat, Amount, Localization.instance.Localize(ItemName), Removals.Count);
                
                foreach (var removal in Removals)
                {
                    if (removal.UsedPlayerInventory)
                    {
                        sb.AppendFormat(colorize ? playerEntryFormatColor : playerEntryFormat, removal.AmountRemoved);
                    }
                    else
                    {
                        sb.AppendFormat(colorize ? containerEntryFormatColor : containerEntryFormat,
                            Localization.instance.Localize(removal.TrackedContainer?.Container.m_name),
                            removal.AmountRemoved);
                    }
                }

                return sb.ToString();
            }
        }
    }
}
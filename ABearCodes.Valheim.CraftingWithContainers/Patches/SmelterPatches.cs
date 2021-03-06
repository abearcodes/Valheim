using ABearCodes.Valheim.CraftingWithContainers.Crafting;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Patches
{
    [HarmonyPatch]
    public class SmelterPatch
    {
        // todo: evaluate if there's a prettier way, this is a complete copy-paste postfix
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Smelter), "OnAddFuel", typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData))]
        private static void SmelterOnAddFuelAreaSearch(Smelter __instance, Switch sw, Humanoid user,
            ItemDrop.ItemData item, ZNetView ___m_nview, bool __result)
        {
            if (__result || !Plugin.Settings.AllowTakeFuelForKilnAndFurnace.Value) return;
            if (__instance.GetFuel() > (double) (__instance.m_maxFuel - 1))
            {
                user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
                return;
            }

            if (!user.GetInventory().HaveItemArea(__instance.m_fuelItem.m_itemData.m_shared.m_name))
            {
                user.Message(MessageHud.MessageType.Center,
                    "$msg_donthaveany " + __instance.m_fuelItem.m_itemData.m_shared.m_name);
                return;
            }

            user.Message(MessageHud.MessageType.Center,
                "$msg_added " + __instance.m_fuelItem.m_itemData.m_shared.m_name);
            user.GetInventory().RemoveItemArea(__instance.m_fuelItem.m_itemData.m_shared.m_name, 1);
            ___m_nview.InvokeRPC("AddFuel");
            __result = true;
        }

        // todo: evaluate if there's a prettier way, this is a complete copy-paste detour
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Smelter), "OnAddOre", typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData))]
        private static void SmelterOnAddOreAreaSearch(Smelter __instance, Switch sw, Humanoid user,
            ItemDrop.ItemData item, ZNetView ___m_nview, bool __result)
        {
            if (__result || !Plugin.Settings.AllowTakeFuelForKilnAndFurnace.Value || !user.IsPlayer()) return;
            var player = (Player) user;
            var containers =
                ContainerTracker.GetViableContainersInRangeForPlayer(player,
                    Plugin.Settings.ContainerLookupRange.Value);
            TrackedContainer? usedContainer = null;
            if (item == null)
                foreach (var trackedContainer in containers)
                {
                    var res = __instance.FindCookableItem(trackedContainer.Container.GetInventory());
                    if (res == null) continue;
                    item = res;
                    usedContainer = trackedContainer;
                    break;
                }

            Plugin.Log.LogDebug(
                $"Found item {item?.m_shared.m_name}. Container? {usedContainer.HasValue} {usedContainer?.OwningPiece.m_name}");
            if (item == null)
            {
                user.Message(MessageHud.MessageType.Center, "$msg_noprocessableitems");
                return;
            }

            if (!__instance.IsItemAllowed(item.m_dropPrefab.name))
            {
                user.Message(MessageHud.MessageType.Center, "$msg_wontwork");
                return;
            }

            ZLog.Log("trying to add " + item.m_shared.m_name);
            if (__instance.GetQueueSize() >= __instance.m_maxOre)
            {
                user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
                return;
            }

            user.Message(MessageHud.MessageType.Center, "$msg_added " + item.m_shared.m_name);
            if (!usedContainer.HasValue)
                user.GetInventory().RemoveItem(item, 1);
            else
                InventoryItemRemover.RemoveFromSpecificContainer(item, usedContainer.Value, player);

            ___m_nview.InvokeRPC("AddOre", (object) item.m_dropPrefab.name);
            __result = true;
        }
    }
}
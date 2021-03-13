using System;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Common;

namespace ABearCodes.Valheim.CraftingWithContainers.Tracking
{
    public class ContainerNetworkExtension
    {
        private readonly Container _container;
        private readonly ZNetView _zNetView;

        public ContainerNetworkExtension(Container container, ZNetView zNetView)
        {
            _container = container;
            _zNetView = zNetView;
        }

        public void Register()
        {
            _zNetView.Register("LockContainersRequest", new Action<long, long, bool>(RPC_LockContainersRequest));
            _zNetView.Register("LockContainersResponse",
                new Action<long, long, bool, bool>(RPC_LockContainersResponse));
        }

        public void RequestContainerLock(long playerId, bool shouldLock)
        {
            _zNetView.InvokeRPC("LockContainersRequest", playerId, shouldLock);
        }

        private void RPC_LockContainersResponse(long uid, long playerId, bool shouldLock, bool granted)
        {
            Plugin.Log.LogDebug($"Received lock response uid:{uid} pid:{playerId} L:{shouldLock} G:{granted}");
            if (!Player.m_localPlayer || Player.m_localPlayer.GetPlayerID() != playerId) return;
            Plugin.Log.LogDebug($"I should handle:{uid} pid:{playerId} L:{shouldLock} G:{granted}");
            if (granted)
                Plugin.Log.LogDebug(
                    $"Actual object {_container.GetInventory().GetAllItems().Sum(a => a.m_stack)} total items on {_zNetView.GetZDO().m_uid}");
        }

        private void RPC_LockContainersRequest(long uid, long playerId, bool shouldLock)
        {
            Plugin.Log.LogDebug("Player " + uid + " wants to craft with " + _container.gameObject.name + "   im: " +
                                ZDOMan.instance.GetMyID());
            if (!_zNetView.IsOwner())
            {
                Plugin.Log.LogDebug("  but im not the owner");
            }
            else if (shouldLock && (_container.IsInUse() || (bool) _container.m_wagon && _container.m_wagon.InUse()) &&
                     uid != ZNet.instance.GetUID())
            {
                Plugin.Log.LogDebug("  in use");
                _zNetView.InvokeRPC(uid, "LockContainersResponse", playerId, shouldLock, false);
            }
            else if (!_container.CheckAccess(playerId))
            {
                Plugin.Log.LogDebug("  not yours");
                _zNetView.InvokeRPC(uid, "LockContainersResponse", playerId, shouldLock, false);
            }
            else
            {
                _container.SetInUse(shouldLock);
                ZDOMan.instance.ForceSendZDO(uid, _zNetView.GetZDO().m_uid);
                _zNetView.GetZDO().SetOwner(uid);
                _zNetView.InvokeRPC(uid, "LockContainersResponse", playerId, shouldLock, true);
            }
        }

        public void Unregister()
        {
            _zNetView.Unregister("LockContainersRequest");
            _zNetView.Unregister("LockContainersResponse");
        }
    }
}
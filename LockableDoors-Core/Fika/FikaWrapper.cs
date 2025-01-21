using Comfort.Common;
using EFT.Interactive;
using Fika.Core.Coop.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using LiteNetLib;
using LockableDoors.Components;
using LockableDoors.Packets;

namespace LockableDoors.Fika
{
    internal class FikaWrapper
    {
        public static bool IAmHost()
        {
            return Singleton<FikaServer>.Instantiated;
        }

        public static string GetRaidId()
        {
            return FikaBackendUtils.GroupId;
        }

        public static void SendDoorLockedStatePacket(string doorId, bool isLocked)
        {
            DoorLockedStatePacket packet = new DoorLockedStatePacket
            {
                DoorId = doorId,
                IsLocked = isLocked
            };
            if (Singleton<FikaServer>.Instantiated)
            {
                Singleton<FikaServer>.Instance.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);
            }
            if (Singleton<FikaClient>.Instantiated)
            {
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered);
            }
        }

        private static void OnDoorLockedStatePacketReceived(DoorLockedStatePacket packet, NetPeer peer)
        {
            Door door = LDSession.GetDoor(packet.DoorId);

            if (door == null)
            {
                throw new System.Exception($"Door by id {packet.DoorId} not found!");
            }

            DoorLock doorLock = door.gameObject.GetOrAddComponent<DoorLock>();

            if (packet.IsLocked)
            {
                doorLock.Lock();
            }
            else
            {
                doorLock.Unlock();
            }

            if (Singleton<FikaServer>.Instantiated)
            {
                Singleton<FikaServer>.Instance.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);
            }
        }

        public static void OnFikaNetManagerCreated(FikaNetworkManagerCreatedEvent managerCreatedEvent)
        {
            managerCreatedEvent.Manager.RegisterPacket<DoorLockedStatePacket, NetPeer>(OnDoorLockedStatePacketReceived);
        }

        public static void InitOnPluginEnabled()
        {
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnFikaNetManagerCreated);
        }
    }
}

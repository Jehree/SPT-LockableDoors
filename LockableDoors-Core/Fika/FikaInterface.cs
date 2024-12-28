using Comfort.Common;
using EFT;

namespace LockableDoors.Fika
{
    internal class FikaInterface
    {
        public static bool IAmHost()
        {
            if (!Plugin.FikaInstalled) return true;
            return FikaWrapper.IAmHost();
        }

        public static string GetRaidId()
        {
            if (!Plugin.FikaInstalled) return Singleton<GameWorld>.Instance.MainPlayer.ProfileId;
            return FikaWrapper.GetRaidId();
        }

        public static void InitOnPluginEnabled()
        {
            if (!Plugin.FikaInstalled) return;
            FikaWrapper.InitOnPluginEnabled();
        }

        public static void SendDoorLockedStatePacket(string doorId, bool isLocked)
        {
            if (!Plugin.FikaInstalled) return;
            FikaWrapper.SendDoorLockedStatePacket(doorId, isLocked);
        }
    }
}

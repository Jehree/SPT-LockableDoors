using EFT;
using EFT.Interactive;
using LockableDoors.Components;
using LockableDoors.Fika;
using LockableDoors.Helpers;
using LockableDoors.Models;
using SPT.Reflection.Patching;
using System.Reflection;

namespace LockableDoors.Patches
{
    internal class GameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPrefix]
        static void PatchPrefix()
        {
            Plugin.LogSource.LogError(FikaInterface.GetRaidId());
            ServerDataPack requestPack = new ServerDataPack(FikaInterface.GetRaidId(), ModSession.Instance.GameWorld.LocationId.ToLower());
            Plugin.LogSource.LogError("1");
            ServerDataPack pack = Utils.ServerRoute<ServerDataPack>(Plugin.DataToClientURL, requestPack);
            Plugin.LogSource.LogError("2");
            foreach (string id in pack.LockedDoorIds)
            {
                Plugin.LogSource.LogError(id);
                Door door = ModSession.Instance.GameWorld.FindDoor(id) as Door;
                Plugin.LogSource.LogError(door == null);

                if (door.DoorState != EDoorState.Shut) continue;

                DoorLock doorLock = door.gameObject.AddComponent<DoorLock>();
                doorLock.Lock();
            }
        }
    }
}

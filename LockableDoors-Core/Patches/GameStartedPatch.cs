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
            ServerDataPack requestPack = new ServerDataPack(FikaInterface.GetRaidId(), ModSession.Instance.GameWorld.LocationId.ToLower());
            ServerDataPack pack = Utils.ServerRoute<ServerDataPack>(Plugin.DataToClientURL, requestPack);
            foreach (string id in pack.LockedDoorIds)
            {
                Door door = ModSession.GetDoor(id);
                if (door.DoorState != EDoorState.Shut) continue;

                DoorLock doorLock = door.gameObject.AddComponent<DoorLock>();
                doorLock.Lock();

                if (Settings.VisualizerEnabled.Value)
                {
                    doorLock.EnabledVisualizer();
                }
            }
        }
    }
}

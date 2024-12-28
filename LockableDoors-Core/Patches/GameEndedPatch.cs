using EFT;
using EFT.Interactive;
using HarmonyLib;
using LockableDoors.Components;
using LockableDoors.Fika;
using LockableDoors.Helpers;
using LockableDoors.Models;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LockableDoors.Patches
{
    internal class GameEndedPatch : ModulePatch
    {
        private static Type _targetClassType;

        protected override MethodBase GetTargetMethod()
        {
            _targetClassType = PatchConstants.EftTypes.Single(targetClass =>
                !targetClass.IsInterface &&
                !targetClass.IsNested &&
                targetClass.GetMethods().Any(method => method.Name == "LocalRaidEnded") &&
                targetClass.GetMethods().Any(method => method.Name == "ReceiveInsurancePrices")
            );

            return AccessTools.Method(_targetClassType.GetTypeInfo(), "LocalRaidEnded");
        }

        // LocalRaidSettings settings, GClass1924 results, GClass1301[] lostInsuredItems, Dictionary<string, GClass1301[]> transferItems
        [PatchPostfix]
        static void Postfix(LocalRaidSettings settings, object results, ref object[] lostInsuredItems, object transferItems)
        {
            if (!FikaInterface.IAmHost()) return;

            List<string> lockedDoorIds = ModSession.Instance.DoorsWithLocks.Values.Where(door => door.DoorState == EDoorState.Locked)
                                                                              .Select(door => door.Id)
                                                                              .ToList();

            ServerDataPack pack = new ServerDataPack(FikaInterface.GetRaidId(), ModSession.Instance.GameWorld.LocationId.ToLower(), lockedDoorIds);
            Utils.ServerRoute(Plugin.DataToServerURL, pack);
        }
    }
}

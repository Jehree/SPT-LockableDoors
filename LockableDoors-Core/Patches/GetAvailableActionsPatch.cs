using EFT.Interactive;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using LockableDoors.Components;

namespace LockableDoors.Patches
{
    internal class GetAvailableActionsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.FirstMethod(typeof(GetActionsClass), method => method.Name == nameof(GetActionsClass.GetAvailableActions) && method.GetParameters()[0].Name == "owner");
        }

        [PatchPostfix]
        static void PatchPostfix(GamePlayerOwner owner, object interactive, ref ActionsReturnClass __result)
        {
            if (interactive is not Door) return;
            Door door = interactive as Door;

            if (door.gameObject.TryGetComponent(out DoorLock doorLock))
            {
                doorLock.AddLockInteractionsToActionList(__result.Actions);
            }
            else
            {
                DoorLock.AddUninitializedLockInteractionsToActionList(__result.Actions, door);
            }
        }
    }
}

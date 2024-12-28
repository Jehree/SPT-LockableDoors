﻿using EFT.Console.Core;
using EFT.UI;
using LockableDoors.Components;
using LockableDoors.Fika;
using LockableDoors.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockableDoors.Common
{
    internal class ConsoleCommands
    {
        [ConsoleCommand("unlock_all_player_locked_doors", "", null, "Unlocks all doors on this map that the player locked via the LockableDoors mod")]
        public static void UnlockAllDoors()
        {
            if (ModSession.Instance == null)
            {
                ConsoleScreen.LogError("Can't do that when not loaded into a raid!");
                return;
            }

            foreach (var kvp in ModSession.Instance.DoorsWithLocks)
            {
                DoorLock doorLock = DoorLock.GetLock(kvp.Value.Id);
                doorLock.Unlock();
                FikaInterface.SendDoorLockedStatePacket(kvp.Value.Id, false);

                if (Settings.VisualizerEnabled.Value)
                {
                    doorLock.DisableVisualizer();
                }
            }

            NotificationManagerClass.DisplayMessageNotification($"All doors unlocked!");
        }
    }
}

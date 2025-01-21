using LockableDoors.Components;
using LockableDoors.Fika;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockableDoors.Models
{
    internal class ServerDataPack
    {
        public string ProfileId;
        public string MapId;
        public List<string> LockedDoorIds = new List<string>();

        public ServerDataPack(string profileId, string mapId, List<string> lockedDoorIds = null)
        {
            ProfileId = profileId;
            MapId = mapId;
            if (lockedDoorIds != null)
            {
                LockedDoorIds = lockedDoorIds;
            }
        }

        public static ServerDataPack GetRequestPack()
        {
            return new ServerDataPack(FikaInterface.GetRaidId(), LDSession.Instance.GameWorld.LocationId.ToLower());
        }
    }
}

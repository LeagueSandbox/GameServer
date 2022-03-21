using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain
{
    public interface IMonsterCamp
    {
        byte CampIndex { get; set; }
        Vector3 Position { get; set; }
        byte SideTeamId { get; set; }
        string MinimapIcon { get; set; }
        byte RevealEvent { get; set; }
        float Expire { get; set; }
        int TimerType { get; set; }
        float SpawnDuration { get; set; }
        bool IsAlive { get; set; }
        float RespawnTimer { get; set; }
        List<IMonster> Monsters { get; }
        IMonster AddMonster(IMonster monster);
        void NotifyCampActivation();
    }
}

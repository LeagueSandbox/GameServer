using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface ILaneMinion : IObjAiBase
    {
        string BarracksName { get; }
        MinionSpawnType MinionSpawnType { get; }
    }
}

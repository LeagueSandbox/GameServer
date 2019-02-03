using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface ILaneMinion : IObjAiBase
    {
        MinionSpawnPosition SpawnPosition { get; }
        MinionSpawnType MinionSpawnType { get; }
    }
}

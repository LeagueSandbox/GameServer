using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IMinion : IObjAiBase
    {
        MinionSpawnPosition SpawnPosition { get; }
        MinionSpawnType MinionSpawnType { get; }
    }
}

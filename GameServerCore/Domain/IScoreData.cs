using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IScoreData
    {
        IChampion Owner { get; }
        float Points { get; }
        ScoreCategory ScoreCategory { get; }
        ScoreEvent ScoreEvent { get; }
        bool DoCallOut { get; }
    }
}

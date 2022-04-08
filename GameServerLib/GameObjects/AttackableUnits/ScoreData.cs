using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerLib.GameObjects.AttackableUnits
{
    class ScoreData : IScoreData
    {
        public IChampion Owner { get; }
        public float Points { get; }
        public ScoreCategory ScoreCategory { get; }
        public ScoreEvent ScoreEvent { get; }
        public bool DoCallOut { get; }
        public ScoreData(IChampion owner, float points, ScoreCategory scoreCategory, ScoreEvent scoreEvent, bool doCallOut)
        {
            Owner = owner;
            Points = points;
            ScoreCategory = scoreCategory;
            ScoreEvent = scoreEvent;
            DoCallOut = doCallOut;
        }
    }
}

using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IChampion : IObjAiBase
    {
        IShop Shop { get; }
        float RespawnTimer { get; }
        int DeathSpree { get; set; }
        int KillSpree { get; set;  }
        float GoldFromMinions { get; set; }
        IRuneCollection RuneList { get; }
        ITalentInventory TalentInventory { get; }
        IChampionStats ChampStats { get; }
        byte SkillPoints { get; set; }

        // basic
        void AddGold(IAttackableUnit source, float gold, bool notify = true);
        void UpdateSkin(int skinNo);
        uint GetPlayerId();
        void AddExperience(float experience, bool notify = true);
        bool LevelUp(bool force = false);
        void Recall();
        void Respawn();
        bool OnDisconnect();
        void AddToolTipChange(IToolTipData data);
        void OnKill(IDeathData deathData);
        void IncrementScore(float points, ScoreCategory scoreCategory, ScoreEvent scoreEvent, bool doCallOut, bool notifyText = true);
    }
}

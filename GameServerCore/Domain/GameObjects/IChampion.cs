using GameServerCore.Enums;
using System.Collections.Generic;
using LeaguePackets.Game.Events;

namespace GameServerCore.Domain.GameObjects
{
    public interface IChampion : IObjAiBase
    {
        int ClientId { get; }
        IShop Shop { get; }
        float RespawnTimer { get; }
        int DeathSpree { get; set; }
        int KillSpree { get; set;  }
        float GoldFromMinions { get; set; }
        IRuneCollection RuneList { get; }
        ITalentInventory TalentInventory { get; }
        IChampionStats ChampStats { get; }
        byte SkillPoints { get; set; }
        List<EventHistoryEntry> EventHistory { get; }

        // basic
        void AddGold(IAttackableUnit source, float gold, bool notify = true);
        void UpdateSkin(int skinNo);
        void AddExperience(float experience, bool notify = true);
        void Recall();
        void Respawn();
        bool OnDisconnect();
        void AddToolTipChange(IToolTipData data);
        void OnKill(IDeathData deathData);
        void IncrementScore(float points, ScoreCategory scoreCategory, ScoreEvent scoreEvent, bool doCallOut, bool notifyText = true);
    }
}

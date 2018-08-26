using System.Collections.Generic;
using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IChampion : IObjAiBase
    {
        float RespawnTimer { get; }
        float ChampionGoldFromMinions { get; }
        IRuneCollection RuneList { get; }
        Dictionary<short, ISpell> Spells { get; }
        int Skin { get; }
        IChampionStats ChampStats { get; }
        byte SkillPoints { get; set; }

        void TeleportTo(float x, float y);
        void UpdateSkin(int skinNo);
        int GetChampionHash();
        void StopChampionMovement();
        bool CanMove();
        void UpdateMoveOrder(MoveOrder order);
        bool CanCast();
        ISpell GetSpell(byte slot);
        ISpell LevelUpSpell(byte slot);
    }
}

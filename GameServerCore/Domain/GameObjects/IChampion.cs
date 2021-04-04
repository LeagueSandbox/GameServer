using System.Collections.Generic;
using GameServerCore.Domain.GameObjects.Spell;

namespace GameServerCore.Domain.GameObjects
{
    public interface IChampion : IObjAiBase
    {
        IShop Shop { get; }
        float RespawnTimer { get; }
        float ChampionGoldFromMinions { get; set; }
        IRuneCollection RuneList { get; }
        int Skin { get; }
        IChampionStats ChampStats { get; }
        byte SkillPoints { get; set; }

        // basic
        void UpdateSkin(int skinNo);
        bool LevelUp();
        void Recall();
        void Respawn();
        bool OnDisconnect();

        void OnKill(IAttackableUnit killed);
    }
}

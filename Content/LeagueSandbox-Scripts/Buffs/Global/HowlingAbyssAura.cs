using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class HowlingAbyssAura : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IChampion Champion;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell = null)
        {
            //TODO: Reduce outgoing heal by 50%, 0.15% max mana as Mana regen
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        float XpCounter = 0;
        public void OnUpdate(float diff)
        {
            XpCounter += diff;
            if(XpCounter > 1000 && Champion != null)
            {
                Champion.AddExperience(5.0f, false);
                XpCounter = 0;
            }
        }
    }
}


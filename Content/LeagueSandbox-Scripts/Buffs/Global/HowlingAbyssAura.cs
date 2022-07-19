using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class HowlingAbyssAura : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Champion Champion;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell = null)
        {
            //TODO: Reduce outgoing heal by 50%, 0.15% max mana as Mana regen
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


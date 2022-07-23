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
    internal class OdinPlayerBuff : IBuffGameScript
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
            if(unit is Champion ch)
            {
                Champion = ch;
            }

            //TODO: Add 2% mana regeneration per 1% missing mana
            if (unit.Stats.ParType == PrimaryAbilityResourceType.Energy)
            {
                StatsModifier.ManaRegeneration.FlatBonus += 2.0f;
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
        }
        float XpCounter = 0;
        public void OnUpdate(float diff)
        {
            XpCounter += diff;
            if(XpCounter > 1000 && Champion != null)
            {
                Champion.AddExperience(7.2f, false);
                XpCounter = 0;
            }
        }
    }
}


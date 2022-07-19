using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class YasuoQ02 : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; }

        private Particle p1;
        private Particle p2;
        private Particle p3;
        private Particle p4;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            var caster = ownerSpell.CastInfo.Owner;
            SetSpell(caster, "YasuoQ3W", SpellSlotType.SpellSlots, 0);
            p1 = AddParticleTarget(caster, caster, "Yasuo_Base_Q3_Indicator_Ring", caster);
            p2 = AddParticleTarget(caster, caster, "Yasuo_Base_Q3_Indicator_Ring_alt", caster);
            p3 = AddParticleTarget(caster, caster, "Yasuo_Base_Q_wind_ready_buff", caster);
            p4 = AddParticleTarget(caster, caster, "Yasuo_Base_Q_strike_build_up_test", caster);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            if (unit is ObjAIBase ai)
            {
                if (ai.Spells[0].SpellName == "YasuoQ3W")
                {
                    SetSpell(ai, "YasuoQW", SpellSlotType.SpellSlots, 0);
                }
            }
            RemoveParticle(p1);
            RemoveParticle(p2);
            RemoveParticle(p3);
            RemoveParticle(p4);
        }
    }
}

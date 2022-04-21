using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class YasuoQ02 : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; }

        private IParticle p1;
        private IParticle p2;
        private IParticle p3;
        private IParticle p4;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var caster = ownerSpell.CastInfo.Owner;
            SetSpell(caster, "YasuoQ3W", SpellSlotType.SpellSlots, 0);
            p1 = AddParticleTarget(caster, caster, "Yasuo_Base_Q3_Indicator_Ring", caster);
            p2 = AddParticleTarget(caster, caster, "Yasuo_Base_Q3_Indicator_Ring_alt", caster);
            p3 = AddParticleTarget(caster, caster, "Yasuo_Base_Q_wind_ready_buff", caster);
            p4 = AddParticleTarget(caster, caster, "Yasuo_Base_Q_strike_build_up_test", caster);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (unit is IObjAiBase ai)
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

        public void OnUpdate(float diff)
        {
            //empty!
        }
    }
}

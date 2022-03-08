using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class OdinSpeedShrineBuff : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.RENEW_EXISTING
        };
        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle p1;
        IParticle p2;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            //TODO: Make it decay over the duration of the buff
            p1 = AddParticleTarget(buff.SourceUnit, unit, "Odin_Speed_Shrine_buf", unit, -1.0f);
            p2 = AddParticleTarget(buff.SourceUnit, unit, "invis_runes_01", unit, -1.0f);
            StatsModifier.MoveSpeed.PercentBonus += 0.3f;
            unit.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(p1);
            RemoveParticle(p2);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}


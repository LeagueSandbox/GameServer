using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class YasuoE : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING,
        };

        public IStatsModifier StatsModifier { get; private set; }

        private readonly IAttackableUnit target = Spells.YasuoDashWrapper._target;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var owner = ownerSpell.CastInfo.Owner;
            var time = 0.6f - ownerSpell.CastInfo.SpellLevel * 0.1f;
            var damage = 50f + ownerSpell.CastInfo.SpellLevel * 20f + unit.Stats.AbilityPower.Total * 0.6f;
            AddParticleTarget(owner, unit, "Yasuo_Base_E_Dash", unit);
            AddParticleTarget(owner, target, "Yasuo_Base_E_dash_hit", target);
            var to = Vector2.Normalize(target.Position - unit.Position);
            ForceMovement(unit, "Spell3", new Vector2(target.Position.X + to.X * 175f, target.Position.Y + to.Y * 175f), 750f + unit.GetTrueMoveSpeed() * 0.6f, 0, 0, 0);
            target.TakeDamage(unit, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            CancelDash(unit);
        }

        public void OnUpdate(float diff)
        {
            //empty
        }
    }
}

using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class YasuoE : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING,
        };

        public StatsModifier StatsModifier { get; private set; }

        private readonly AttackableUnit target = Spells.YasuoDashWrapper._target;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            var owner = ownerSpell.CastInfo.Owner;
            var time = 0.6f - ownerSpell.CastInfo.SpellLevel * 0.1f;
            var damage = 50f + ownerSpell.CastInfo.SpellLevel * 20f + unit.Stats.AbilityPower.Total * 0.6f;
            AddParticleTarget(owner, unit, "Yasuo_Base_E_Dash", unit);
            AddParticleTarget(owner, target, "Yasuo_Base_E_dash_hit", target);
            var to = Vector2.Normalize(target.Position - unit.Position);
            ForceMovement(unit, "Spell3", new Vector2(target.Position.X + to.X * 175f, target.Position.Y + to.Y * 175f), 750f + unit.Stats.GetTrueMoveSpeed() * 0.6f, 0, 0, 0);
            target.TakeDamage(unit, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            CancelDash(unit);
        }
    }
}

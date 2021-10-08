using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;

namespace Buffs
{
    internal class AnnieEBuff : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_ENCHANCER;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle _particle;
        ISpell _ownerSpell;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _ownerSpell = ownerSpell;
            _particle = AddParticleTarget(unit, unit, "Annie_E_buf.troy", unit, buff.Duration);

            float bonus = 10f + (10f * ownerSpell.CastInfo.Owner.Stats.Level);
            StatsModifier.Armor.FlatBonus += bonus;
            StatsModifier.MagicResist.FlatBonus += bonus;
            unit.AddStatModifier(StatsModifier);

            ApiEventManager.OnTakeDamage.AddListener(unit, unit, OnTakeDamage, false);
        }

        public void OnTakeDamage(IAttackableUnit unit, IAttackableUnit attacker, ISpell spell)
        {
            if (spell.CastInfo.IsAutoAttack && !(attacker is IBaseTurret))
            {
                float ap = _ownerSpell.CastInfo.Owner.Stats.AbilityPower.Total * 0.2f;
                float damage = ap + 10f + (_ownerSpell.CastInfo.SpellLevel * 10f);

                attacker.TakeDamage(_ownerSpell.CastInfo.Owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_ATTACK, 
                    DamageResultType.RESULT_NORMAL, _ownerSpell);

                AddParticleTarget(attacker, attacker, "global_armor_pos_buf.troy", attacker);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(_particle);
            ApiEventManager.OnTakeDamage.RemoveListener(unit);
        }

        public void OnUpdate(float diff)
        {

        }
    }
}

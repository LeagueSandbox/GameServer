using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class MoltenShield : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            float bonus = 10f + (10f * ownerSpell.CastInfo.Owner.Stats.Level);
            
            StatsModifier.Armor.FlatBonus += bonus;
            StatsModifier.MagicResist.FlatBonus += bonus;
            unit.AddStatModifier(StatsModifier);

            // TODO: Use OnPreDamage instead.
            ApiEventManager.OnBeingHit.AddListener(this, unit, OnBeingHit, false);
        }

        public void OnBeingHit(IAttackableUnit owner, IAttackableUnit attacker)
        {
            if (!(attacker is IBaseTurret) && owner is IObjAiBase obj)
            {
                float ap = obj.Stats.AbilityPower.Total * 0.2f;
                float damage = ap + 10f + (obj.Spells[2].CastInfo.SpellLevel * 10f);

                attacker.TakeDamage(obj, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                AddParticleTarget(obj, null, "global_armor_pos_buf", attacker);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ApiEventManager.OnBeingHit.RemoveListener(this);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
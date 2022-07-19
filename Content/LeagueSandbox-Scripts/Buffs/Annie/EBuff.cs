using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace Buffs
{
    internal class MoltenShield : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            float bonus = 10f + (10f * ownerSpell.CastInfo.Owner.Stats.Level);
            
            StatsModifier.Armor.FlatBonus += bonus;
            StatsModifier.MagicResist.FlatBonus += bonus;
            unit.AddStatModifier(StatsModifier);

            // TODO: Use OnPreDamage instead.
            ApiEventManager.OnBeingHit.AddListener(this, unit, OnBeingHit, false);
        }

        public void OnBeingHit(AttackableUnit owner, AttackableUnit attacker)
        {
            if (!(attacker is BaseTurret) && owner is ObjAIBase obj)
            {
                float ap = obj.Stats.AbilityPower.Total * 0.2f;
                float damage = ap + 10f + (obj.Spells[2].CastInfo.SpellLevel * 10f);

                attacker.TakeDamage(obj, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                AddParticleTarget(obj, null, "global_armor_pos_buf", attacker);
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            ApiEventManager.OnBeingHit.RemoveListener(this);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
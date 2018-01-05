using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.API;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class AttackableUnit : Unit
    {
        public AttackableUnit(string model, Stats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) :
            base(model, stats, collisionRadius, x, y, visionRadius, netId) { }
        
        
        public virtual void TakeDamage(Unit attacker, float damage, DamageType type, DamageSource source,
            DamageText damageText)
        {
            float defense = 0;
            float regain = 0;
            var attackerStats = attacker.GetStats();

            switch (type)
            {
                case DamageType.DAMAGE_TYPE_PHYSICAL:
                    defense = GetStats().Armor.Total;
                    defense = (1 - attackerStats.ArmorPenetration.PercentBonus) * defense -
                              attackerStats.ArmorPenetration.FlatBonus;

                    break;
                case DamageType.DAMAGE_TYPE_MAGICAL:
                    defense = GetStats().MagicPenetration.Total;
                    defense = (1 - attackerStats.MagicPenetration.PercentBonus) * defense -
                              attackerStats.MagicPenetration.FlatBonus;
                    break;
                case DamageType.DAMAGE_TYPE_TRUE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            switch (source)
            {
                case DamageSource.DAMAGE_SOURCE_SPELL:
                    regain = attackerStats.SpellVamp.Total;
                    break;
                case DamageSource.DAMAGE_SOURCE_ATTACK:
                    regain = attackerStats.LifeSteal.Total;
                    break;
                case DamageSource.DAMAGE_SOURCE_SUMMONER_SPELL:
                    break;
                case DamageSource.DAMAGE_SOURCE_PASSIVE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }

            //Damage dealing. (based on leagueoflegends' wikia)
            damage = defense >= 0 ? (100 / (100 + defense)) * damage : (2 - (100 / (100 - defense))) * damage;

            if (HasCrowdControl(CrowdControlType.Invulnerable))
            {
                bool attackerIsFountainTurret;
                var checkLaneTurret = attacker as LaneTurret;

                if (checkLaneTurret != null)
                {
                    attackerIsFountainTurret = checkLaneTurret.Type == TurretType.FountainTurret;
                }
                else
                {
                    attackerIsFountainTurret = false;
                }

                if (attackerIsFountainTurret == false)
                {
                    damage = 0;
                    damageText = DamageText.DAMAGE_TEXT_INVULNERABLE;
                }
            }

            ApiEventManager.OnUnitDamageTaken.Publish(this);

            GetStats().CurrentHealth = Math.Max(0.0f, GetStats().CurrentHealth - damage);
            if (!IsDead && GetStats().CurrentHealth <= 0)
            {
                IsDead = true;
                die(attacker);
            }

            _game.PacketNotifier.NotifyDamageDone(attacker, this, damage, type, damageText);
            _game.PacketNotifier.NotifyUpdatedStats(this, false);

            // Get health from lifesteal/spellvamp
            if (regain > 0)
            {
                attackerStats.CurrentHealth = Math.Min(attackerStats.HealthPoints.Total,
                    attackerStats.CurrentHealth + regain * damage);
                _game.PacketNotifier.NotifyUpdatedStats(attacker, false);
            }
        }

        public virtual void TakeDamage(Unit attacker, float damage, DamageType type, DamageSource source, bool isCrit)
        {
            var text = DamageText.DAMAGE_TEXT_NORMAL;

            if (isCrit)
            {
                text = DamageText.DAMAGE_TEXT_CRITICAL;
            }

            TakeDamage(attacker, damage, type, source, text);
        }
    }

}
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Linq;
using GameServerCore;

namespace Spells
{
    public class KarthusLayWasteA1 : IGameScript
    {
        public void OnActivate(IObjAiBase owner)
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            AddParticleTarget(owner, "Karthus_Base_Q_Hand_Glow.troy", owner, 1, "R_Hand");
            AddParticle(owner, "Karthus_Base_Q_Point.troy", spell.X, spell.Y);
            AddParticle(owner, "Karthus_Base_Q_Ring.troy", spell.X, spell.Y);
            AddParticle(owner, "Karthus_Base_Q_Skull_Child.troy", spell.X, spell.Y);
        }   

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            IGameObject m = AddParticle(owner, "Karthus_Base_Q_Explosion.troy", spell.X, spell.Y);
            var affectedUnits = GetUnitsInRange(m, 150, true);
            var ap = owner.Stats.AbilityPower.Total;
            var damage = 20f + spell.Level * 20f + ap * 0.3f;
            if (affectedUnits.Count == 0)
            {
                AddParticle(owner, "Karthus_Base_Q_Hit_Miss.troy", spell.X, spell.Y);
            }                        
            foreach (var unit in affectedUnits
            .Where(x => x.Team == CustomConvert.GetEnemyTeam(owner.Team)))
            {
                if (unit is IChampion || unit is IMinion)
                {                        
                    if (affectedUnits.Count == 1)
                    {
                        damage *= 2;
                        AddParticle(owner, "Karthus_Base_Q_Hit_Single.troy", spell.X, spell.Y);
                        unit.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, true);
                    }
                    if (affectedUnits.Count > 1)
                    {
                        AddParticle(owner, "Karthus_Base_Q_Hit_Many.troy", spell.X, spell.Y);
                        unit.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                    }
                }
            }
            m.SetToRemove();
            AddParticle(owner, "Karthus_Base_Q_Explosion_Sound.troy", spell.X, spell.Y);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }
    }
}

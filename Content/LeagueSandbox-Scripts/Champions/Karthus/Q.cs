using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Linq;
using GameServerCore;
using System.Numerics;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class KarthusLayWasteA1 : IGameScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            AddParticleTarget(owner, "Karthus_Base_Q_Hand_Glow.troy", owner, 1, "R_Hand");
            AddParticle(owner, "Karthus_Base_Q_Point.troy", spellPos);
            AddParticle(owner, "Karthus_Base_Q_Ring.troy", spellPos);
            AddParticle(owner, "Karthus_Base_Q_Skull_Child.troy", spellPos);
        }   

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            IGameObject m = AddParticle(owner, "Karthus_Base_Q_Explosion.troy", spellPos);
            var affectedUnits = GetUnitsInRange(m.Position, 150, true);
            var ap = owner.Stats.AbilityPower.Total;
            var damage = 20f + spell.CastInfo.SpellLevel * 20f + ap * 0.3f;
            if (affectedUnits.Count == 0)
            {
                AddParticle(owner, "Karthus_Base_Q_Hit_Miss.troy", spellPos);
            }                        
            foreach (var unit in affectedUnits
            .Where(x => x.Team == CustomConvert.GetEnemyTeam(owner.Team)))
            {
                if (unit is IChampion || unit is IMinion)
                {                        
                    if (affectedUnits.Count == 1)
                    {
                        damage *= 2;
                        AddParticle(owner, "Karthus_Base_Q_Hit_Single.troy", spellPos);
                        unit.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, true);
                    }
                    if (affectedUnits.Count > 1)
                    {
                        AddParticle(owner, "Karthus_Base_Q_Hit_Many.troy", spellPos);
                        unit.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                    }
                }
            }
            m.SetToRemove();
            AddParticle(owner, "Karthus_Base_Q_Explosion_Sound.troy", spellPos);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

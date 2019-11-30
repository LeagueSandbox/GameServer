using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Spells
{
    public class StaticField : ISpellScript
    {
        Particle ultiReady;

        public void OnActivate(IChampion owner)
        {
            //ultiReady = AddParticleTarget(owner, "StaticField_ready.troy", owner, 1, "C_BUFFBONE_GLB_CHEST_LOC", 0x20);
        }

        public void OnDeactivate(IChampion owner)
        {
            //RemoveParticle(ultiReady);
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            spell.SpellAnimation("Spell4", owner);
            //owner.
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            AddParticleTarget(owner, "staticfield_nova.troy", owner, 1, "C_BUFFBONE_GLB_CHEST_LOC");

            var damage = 125 + 125 * spell.Level + owner.Stats.AbilityPower.Total;
            foreach (var enemyTarget in GetUnitsInRange(owner, 600, true).Where(x => x.Team == CustomConvert.GetEnemyTeam(owner.Team)))
            {
                enemyTarget.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            }
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            //if(!owner.Team.Equals(target.Team))
            //    AddParticleTarget(owner, "StaticField_hit.troy", target, 1, "");
        }

        public void CooldownStarted(IChampion owner, ISpell spell)
        {
            
        }

        public void CooldownEnded(IChampion owner, ISpell spell)
        {
            
        }
    }
}

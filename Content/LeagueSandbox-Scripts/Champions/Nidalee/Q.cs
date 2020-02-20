using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System;

namespace Spells
{
    public class JavelinToss : IGameScript
    {
        public float finaldamage;
        public Vector2 castcoords;
        public void OnActivate(IObjAiBase owner)
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            castcoords = new Vector2(owner.X, owner.Y);
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - castcoords);
            var range = to * 1500f;
            var trueCoords = castcoords + range;
            spell.AddProjectile("JavelinToss", castcoords.X, castcoords.Y, trueCoords.X, trueCoords.Y, true);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            var ap = owner.Stats.AbilityPower.Total;
            var basedamage = 25 + spell.Level * 55 + ap;
            var hitcoords = new Vector2(projectile.X, projectile.Y);
            var distance = Math.Sqrt(Math.Pow((castcoords.X - hitcoords.X), 2) + Math.Pow((castcoords.Y - hitcoords.Y), 2));
            if (Math.Abs(distance) <= 525f)
            {
                finaldamage = basedamage;
            }
            else if (distance > 525f && !(distance >= 1300f))
            {
                var damagerampup = (basedamage * (0.02f * (float)Math.Round(Math.Abs(distance - 525f) / 7.75f)));
                finaldamage = basedamage + damagerampup;
            }
            else if (distance >= 1300f)
            {
                finaldamage = (basedamage + (basedamage * 2));
            }
            target.TakeDamage(owner, finaldamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            if (!target.IsDead)
            {
                AddParticleTarget(owner, "Nidalee_Base_Q_Tar.troy", target, 1, "C_BUFFBONE_GLB_CHEST_LOC");
            }

            projectile.SetToRemove();
        }

        public void OnUpdate(double diff)
        {
        }
    }
}


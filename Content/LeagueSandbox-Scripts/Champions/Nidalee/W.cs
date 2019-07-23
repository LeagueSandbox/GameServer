using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Spells
{
    class Bushwhack : IGameScript
    {
        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var spellCastRange = spell.SpellData.CastRange[0];
            var spellDotDamage = (0.08f + 0.02f * spell.Level) / 5;

            var bushwackDuration = 120f;

            var ownerPos = new Vector2(owner.X, owner.Y);
            var spellPos = new Vector2(spell.X, spell.Y);

            if (!owner.WithinRange(ownerPos, spellPos, spellCastRange))
            {
                return;
            }

            IMinion bushwackMinion = AddMinion(owner, "Bushwhack", "Bushwhack", spellPos.X, spellPos.Y);

            AddParticle(owner, "Nidalee_Base_W_Cas.troy", spell.X, spell.Y);

            for (float i = 1.0f; i < 5; ++i)
            {
                CreateTimer(1.0f * i, () =>
                {
                    var unitDamagePerDot = target.Stats.CurrentHealth * spellDotDamage;

                    target.TakeDamage(owner, unitDamagePerDot, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL,
                        false);
                });
            }

            CreateTimer(bushwackDuration, () =>
            {
                if (!bushwackMinion.IsDead)
                {
                    bushwackMinion.Die(owner);
                }
            });
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            
        }

        public void OnActivate(IChampion owner)
        {
            
        }

        public void OnDeactivate(IChampion owner)
        {
            
        }
    }
}

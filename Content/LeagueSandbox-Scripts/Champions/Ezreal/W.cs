using System.Numerics;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class EzrealEssenceFlux : IGameScript
    {
        public void OnActivate(IObjAiBase owner)
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            AddParticleTarget(owner, "ezreal_bow_yellow.troy", owner, 1, "L_HAND");
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var current = new Vector2(owner.Position.X, owner.Position.Y);
            var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
            var range = to * 1000;
            var trueCoords = current + range;
            spell.AddProjectile("EzrealEssenceFluxMissile", current, trueCoords);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            var champion = target as IChampion;

            if (champion == null)
            {
                return;
            }

            var buffTime = 5f;
            var ownerAbilityPowerTotal = owner.Stats.AbilityPower.Total;

            if (champion.Team.Equals(owner.Team) && !champion.Equals(owner))
            {
                AddBuff("EzrealWBuff", buffTime, 1, spell, champion, owner);
            }
            else
            {
                var damage = 25 + (45 * spell.Level) + (ownerAbilityPowerTotal * 0.8f);

                target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, DamageResultType.RESULT_NORMAL);
            }
        }

        public void OnUpdate(double diff)
        {
        }
    }
}

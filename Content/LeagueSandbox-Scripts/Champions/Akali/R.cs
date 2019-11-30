using System.Numerics;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameMaths;

namespace Spells
{
    public class AkaliShadowDance : ISpellScript
    {
        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var ownerPos = new Vector2(owner.X, owner.Y);
            var targetPos = new Vector2(target.X, target.Y);
            //TODO: Feels like some effect is missing
            var trueCoords = ownerPos.ExtendInDirection(targetPos, ownerPos.Distance(targetPos) - 80);

            DashToLocation(owner, trueCoords.X, trueCoords.Y, 2200, false, "Spell4", completionHandler:() => ApplyEffects(owner, target, spell, null));
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            var bonusAd = owner.Stats.AttackDamage.Total - owner.Stats.AttackDamage.BaseValue;
            var ap = owner.Stats.AbilityPower.Total * 0.9f;
            var damage = 200 + spell.Level * 150 + bonusAd + ap;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            AddParticleTarget(owner, "akali_shadowDance_tar.troy", target, 1, "");
        }

        public void CooldownStarted(IChampion owner, ISpell spell)        {            //Executed once spell cooldown started        }

        public void CooldownEnded(IChampion owner, ISpell spell)
        {
            //Executed when cooldown is over
        }
    }
}

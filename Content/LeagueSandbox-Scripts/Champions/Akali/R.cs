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
    public class AkaliShadowDance : IGameScript
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
            var to = Vector2.Normalize(targetPos - ownerPos);
            var range = to * (ownerPos.Distance(targetPos) - 80);

            var trueCoords = ownerPos + range;

            //TODO: Dash to the correct location (in front of the enemy IChampion) instead of far behind or inside them
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

        public void OnUpdate(double diff)
        {
        }
    }
}

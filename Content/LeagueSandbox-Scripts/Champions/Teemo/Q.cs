using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class BlindingDart : IGameScript
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
            spell.AddProjectileTarget("ToxicShot", target);
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            var ap = owner.Stats.AbilityPower.Total * 0.8f;
            var damage = 35 + spell.Level * 45 + ap;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            var time = 1.25f + 0.25f * spell.Level;
            ((ObjAiBase) target).AddBuffGameScript("Blind", "Blind", spell, time);
            AddBuffHudVisual("Blind", time, 1, BuffType.COMBAT_DEHANCER, (ObjAiBase) target, time);
            projectile.SetToRemove();
        }

        public void OnUpdate(double diff)
        {
        }
    }
}

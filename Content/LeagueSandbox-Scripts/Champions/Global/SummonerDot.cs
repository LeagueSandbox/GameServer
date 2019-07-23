using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class SummonerDot : IGameScript
    {
        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var visualBuff = AddBuffHudVisual("SummonerDot", 4.0f, 1, BuffType.COMBAT_DEHANCER, (ObjAiBase) target, 4.0f);
            var p = AddParticleTarget(owner, "Global_SS_Ignite.troy", target, 1);
            var damage = 10 + owner.Stats.Level * 4;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_SPELL, false);
            for(float i = 1.0f; i < 5; ++i)
            {
                CreateTimer(1.0f * i, () =>
                {
                    target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_SPELL,
                        false);
                });
            }
            CreateTimer(4.0f, () =>
            {
                RemoveParticle(p);
            });
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
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


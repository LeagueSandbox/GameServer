using System.Linq;
using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace Spells
{
    public class Shatter : IGameScript
    {
        public void OnActivate(IObjAiBase owner)
        {

        }

        public void OnDeactivate(IObjAiBase owner)
        {

        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {

        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var armor = owner.Stats.Armor.Total;
            var damage = spell.Level * 40 + armor * 0.2f;
            var reduce = spell.Level * 5 + armor * 0.05f;
            AddParticleTarget(owner, "Shatter_nova.troy", owner, 1);

            foreach (var enemy in GetUnitsInRange(owner, 375, true)
                .Where(x => x.Team == CustomConvert.GetEnemyTeam(owner.Team)))
            {
                var hasbuff = HasBuff((IObjAiBase)enemy, "TaricWDis");
                if (enemy is IObjAiBase)
                {
                    enemy.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                    var p2 = AddParticleTarget(owner, "Shatter_tar.troy", enemy, 1);
                    AddBuff("TaricWDis", 4.0f, 1, spell, (IObjAiBase)enemy, owner);

                    if (hasbuff == true)
                    {
                        return;
                    }
                    if (hasbuff == false)
                    {
                        enemy.Stats.Armor.FlatBonus -= reduce;
                    }

                    CreateTimer(4f, () =>
                    {
                        enemy.Stats.Armor.FlatBonus += reduce;
                        RemoveParticle(p2);
                    });
                }
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {

        }

        public void OnUpdate(double diff)
        {

        }
    }
}

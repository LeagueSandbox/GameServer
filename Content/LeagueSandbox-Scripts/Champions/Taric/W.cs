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
            var armor = owner.Stats.Armor.Total;
            var damage = spell.Level * 40 + armor * 0.2f;
            var reduce = spell.Level * 5 + armor * 0.05f;
            AddParticleTarget(owner, "Shatter_nova.troy", owner, 1);

            foreach (var enemys in GetUnitsInRange(owner, 375, true)
                .Where(x => x.Team == CustomConvert.GetEnemyTeam(owner.Team)))
            {
                var hasbuff = ((ObjAiBase)enemys).HasBuffGameScriptActive("TaricWDis", "TaricWDis");
                if (enemys is IAttackableUnit)
                {
                    enemys.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                    var p2 = AddParticleTarget(owner, "Shatter_tar.troy", enemys, 1);
                    ((ObjAiBase)enemys).AddBuffGameScript("TaricWDis", "TaricWDis", spell, 4f, true);

                    if (hasbuff == true)
                    {
                        return;
                    }
                    if (hasbuff == false)
                    {
                        enemys.Stats.Armor.FlatBonus -= reduce;
                    }

                    CreateTimer(4f, () =>
                    {
                        enemys.Stats.Armor.FlatBonus += reduce;
                        RemoveParticle(p2);
                    });
                }
            }
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {

        }

        public void OnUpdate(double diff)
        {

        }
    }
}

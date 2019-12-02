using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class LuluW : IGameScript
    {
        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            spell.SpellAnimation("SPELL2", owner);
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var IChampion = (IChampion) target;
            if (IChampion.Team != owner.Team)
            {
                spell.AddProjectileTarget("LuluWTwo", target);
            }
            else
            {
                var p1 = AddParticleTarget(owner, "Lulu_W_buf_02.troy", target, 1);
                var p2 = AddParticleTarget(owner, "Lulu_W_buf_01.troy", target, 1);
                var time = 2.5f + 0.5f * spell.Level;
                ((ObjAiBase) target).AddBuffGameScript("LuluWBuff", "LuluWBuff", spell, time, true);
                CreateTimer(time, () =>
                {
                    RemoveParticle(p1);
                    RemoveParticle(p2);
                });
            }
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            var champion = target as IChampion;
            if (champion == null)
                return;
            var time = 1 + 0.25f * spell.Level;
            champion.AddBuffGameScript("LuluWDebuff", "LuluWDebuff", spell, time, true);
            var model = champion.Model;
            ChangeModel(owner.Skin, target);

            var p = AddParticleTarget(owner, "Lulu_W_polymorph_01.troy", target, 1);
            CreateTimer(time, () =>
            {
                RemoveParticle(p);
                champion.ChangeModel(model);
            });
            projectile.SetToRemove();
        }

        public void OnUpdate(double diff)
        {
        }

        private void ChangeModel(int skinId, IAttackableUnit target)
        {
            switch (skinId)
            {
                case 0:
                    target.ChangeModel("LuluSquill");
                    break;
                case 1:
                    target.ChangeModel("LuluCupcake");
                    break;
                case 2:
                    target.ChangeModel("LuluKitty");
                    break;
                case 3:
                    target.ChangeModel("LuluDragon");
                    break;
                case 4:
                    target.ChangeModel("LuluSnowman");
                    break;
            }
        }
    }
}

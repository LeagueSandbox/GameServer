using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class LuluW : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            spell.SpellAnimation("SPELL2", owner);
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var IChampion = (IChampion)target;
            if (IChampion.Team != owner.Team)
            {
                spell.AddProjectileTarget("LuluWTwo", spell.CastInfo.SpellCastLaunchPosition, target);
            }
            else
            {
                var p1 = AddParticleTarget(owner, "Lulu_W_buf_02.troy", target, 1);
                var p2 = AddParticleTarget(owner, "Lulu_W_buf_01.troy", target, 1);
                var time = 2.5f + 0.5f * spell.CastInfo.SpellLevel;
                AddBuff("LuluWBuff", time, 1, spell, target, owner);
                CreateTimer(time, () =>
                {
                    RemoveParticle(p1);
                    RemoveParticle(p2);
                });
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            var champion = target as IChampion;
            if (champion == null)
                return;
            var time = 1 + 0.25f * spell.CastInfo.SpellLevel;
            AddBuff("LuluWDebuff", time, 1, spell, champion, owner);
            var model = champion.Model;
            ChangeModel((owner as IChampion).Skin, target);

            var p = AddParticleTarget(owner, "Lulu_W_polymorph_01.troy", target, 1);
            CreateTimer(time, () =>
            {
                RemoveParticle(p);
                champion.ChangeModel(model);
            });
            projectile.SetToRemove();
        }

        public void OnUpdate(float diff)
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

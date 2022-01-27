using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;

namespace Spells
{
    public class LuluW : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            //owner.SpellAnimation("SPELL2");
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
            if (spell.CastInfo.Targets[0].Unit.Team != spell.CastInfo.Owner.Team)
            {
                //spell.AddProjectileTarget("LuluWTwo", spell.CastInfo.SpellCastLaunchPosition, spell.CastInfo.Targets[0].Unit);
            }
            else
            {
                var time = 2.5f + 0.5f * spell.CastInfo.SpellLevel;
                AddBuff("LuluWBuff", time, 1, spell, spell.CastInfo.Targets[0].Unit, spell.CastInfo.Owner);
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile missile)
        {
            var champion = target as IChampion;
            if (champion == null)
                return;
            var time = 1 + 0.25f * spell.CastInfo.SpellLevel;
            AddBuff("LuluWDebuff", time, 1, spell, champion, owner);
            var model = champion.Model;
            ChangeModel(owner.SkinID, target);

            var p = AddParticleTarget(owner, owner, "Lulu_W_polymorph_01", target);
            CreateTimer(time, () =>
            {
                RemoveParticle(p);
                champion.ChangeModel(model);
            });
            missile.SetToRemove();
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
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

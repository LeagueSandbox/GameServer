using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;

namespace Spells
{
    public class LuluW : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            //owner.SpellAnimation("SPELL2");
        }

        public void OnSpellPostCast(Spell spell)
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

        public void ApplyEffects(ObjAIBase owner, AttackableUnit target, Spell spell, SpellMissile missile)
        {
            var champion = target as Champion;
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

        private void ChangeModel(int skinId, AttackableUnit target)
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

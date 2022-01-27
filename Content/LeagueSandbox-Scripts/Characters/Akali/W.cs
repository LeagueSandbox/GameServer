using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;

namespace Spells
{
    public class AkaliSmokeBomb : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
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
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            var smokeBomb = AddParticle(owner, null, "akali_smoke_bomb_tar", owner.Position);
            /*
             * TODO: Display green border (akali_smoke_bomb_tar_team_green.troy) for the own team,
             * display red border (akali_smoke_bomb_tar_team_red.troy) for the enemy team
             * Currently only displaying the green border for everyone
            */
            var smokeBombBorder = AddParticle(owner, null, "akali_smoke_bomb_tar_team_green", owner.Position);
            //TODO: Add invisibility

            CreateTimer(8.0f, () =>
            {
                LogInfo("8 second timer finished, removing smoke bomb");
                RemoveParticle(smokeBomb);
                RemoveParticle(smokeBombBorder);
                //TODO: Remove invisibility
            });
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
    }
}

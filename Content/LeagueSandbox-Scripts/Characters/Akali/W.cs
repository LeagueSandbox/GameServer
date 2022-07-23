using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class AkaliSmokeBomb : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnSpellPostCast(Spell spell)
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
    }
}

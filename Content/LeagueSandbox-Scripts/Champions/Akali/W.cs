using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class AkaliSmokeBomb : IGameScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var smokeBomb = AddParticle(owner, "akali_smoke_bomb_tar.troy", owner.Position);
            /*
             * TODO: Display green border (akali_smoke_bomb_tar_team_green.troy) for the own team,
             * display red border (akali_smoke_bomb_tar_team_red.troy) for the enemy team
             * Currently only displaying the green border for everyone
            */
            var smokeBombBorder = AddParticle(owner, "akali_smoke_bomb_tar_team_green.troy", owner.Position);
            //TODO: Add invisibility

            CreateTimer(8.0f, () =>
            {
                LogInfo("8 second timer finished, removing smoke bomb");
                RemoveParticle(smokeBomb);
                RemoveParticle(smokeBombBorder);
                //TODO: Remove invisibility
            });
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

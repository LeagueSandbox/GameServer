using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class AkaliSmokeBomb : IGameScript
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
            var smokeBomb = AddParticle(owner, "akali_smoke_bomb_tar.troy", owner.X, owner.Y);
            /*
             * TODO: Display green border (akali_smoke_bomb_tar_team_green.troy) for the own team,
             * display red border (akali_smoke_bomb_tar_team_red.troy) for the enemy team
             * Currently only displaying the green border for everyone
            */
            var smokeBombBorder = AddParticle(owner, "akali_smoke_bomb_tar_team_green.troy", owner.X, owner.Y);
            //TODO: Add invisibility

            CreateTimer(8.0f, () =>
            {
                LogInfo("8 second timer finished, removing smoke bomb");
                RemoveParticle(smokeBomb);
                RemoveParticle(smokeBombBorder);
                //TODO: Remove invisibility
            });
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }
    }
}

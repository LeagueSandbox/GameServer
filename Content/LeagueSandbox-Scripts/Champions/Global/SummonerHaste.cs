using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class SummonerHaste : IGameScript
    {
        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            AddBuff("SummonerHasteBuff", 10.0f, 1, spell, owner, owner);
            var p1 = AddParticleTarget(owner, "Global_SS_Ghost.troy", target);
            var p2 = AddParticleTarget(owner, "Global_SS_Ghost_cas.troy", target);
            CreateTimer(10.0f, () =>
            {
                RemoveParticle(p1);
                RemoveParticle(p2);
            });
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }

        public void OnActivate(IObjAiBase owner)
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }
    }
}


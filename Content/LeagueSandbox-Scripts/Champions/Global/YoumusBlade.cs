using GameServerCore.Domain.GameObjects;
﻿using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class YoumusBlade : IGameScript
    {
        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            owner.AddBuffGameScript("YoumuusGhostblade", "YoumuusGhostblade", spell, 6.0f, true);
            var p = AddParticleTarget(owner, "spectral_fury_activate_speed.troy", owner, 2);
            CreateTimer(6.0f, () =>
            {
                RemoveParticle(p);
            });
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }

        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }
    }
}

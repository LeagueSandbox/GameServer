using System;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Recall
{
    class Recall : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_DEHANCER;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; }

        private IParticle _createdParticle;
        private IChampion owner;
        private IBuff sourceBuff;

        private bool _isSecondPhase = false;

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
            IChampion champion = unit as IChampion;
            owner = champion;
            sourceBuff = buff;

            _createdParticle = AddParticleTarget(champion, "TeleportHome.troy", champion, lifetime: 2);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            LogInfo("sourceBuff.TimeElapsed: " + sourceBuff.TimeElapsed);
            if (sourceBuff.TimeElapsed >= sourceBuff.Duration)
            {
                owner.Recall();
            }
            RemoveParticle(_createdParticle);
        }

        public void OnUpdate(double diff)
        {
            if (owner.IsMovementUpdated())
            {
                sourceBuff.DeactivateBuff();
                return;
            }

            if (sourceBuff.TimeElapsed < 4f && _createdParticle.GetTimeAlive() >= 2f && !_isSecondPhase)
            {
                RemoveParticle(_createdParticle);
                _createdParticle = AddParticleTarget(owner, "TeleportHome.troy", owner, lifetime: 2);
            }
            else if (sourceBuff.TimeElapsed >= 4f && !_isSecondPhase)
            {
                RemoveParticle(_createdParticle);
                _isSecondPhase = true;
                _createdParticle = AddParticleTarget(owner, "TeleportHome.troy", owner, lifetime: 4);
            }
        }
    }
}

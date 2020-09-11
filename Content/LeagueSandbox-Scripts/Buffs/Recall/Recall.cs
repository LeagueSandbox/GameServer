using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System;
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

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
            IChampion champion = unit as IChampion;
            owner = champion;
            sourceBuff = buff;

            _createdParticle = AddParticleTarget(champion, "TeleportHome.troy", champion, 1, "", default(System.Numerics.Vector3), 8.0f);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            owner.Recall();
            RemoveParticle(_createdParticle);
        }

        public void OnUpdate(double diff)
        {
            if (owner.IsMovementUpdated())
            {
                sourceBuff.DeactivateBuff();
            }
        }
    }
}

using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
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

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            IChampion champion = unit as IChampion;
            owner = champion;
            sourceBuff = buff;

            ApiEventManager.OnTakeDamage.AddListener(this, unit, OnTakeDamage, true);
            ApiEventManager.OnUnitUpdateMoveOrder.AddListener(this, champion, OnUpdateMoveOrder, true);

            _createdParticle = AddParticleTarget(champion, "TeleportHome.troy", champion, lifetime: buff.Duration);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (sourceBuff.TimeElapsed >= sourceBuff.Duration)
            {
                owner.Recall();
            }
            RemoveParticle(_createdParticle);
        }

        public void OnTakeDamage(IAttackableUnit unit, IAttackableUnit source)
        {
            var buff = unit.GetBuffWithName("Recall");
            if (buff != null)
            {
                buff.DeactivateBuff();
            }
        }

        public void OnUpdateMoveOrder(IObjAiBase unit)
        {
            var buff = unit.GetBuffWithName("Recall");
            if (buff != null)
            {
                buff.DeactivateBuff();
            }
        }

        public void OnUpdate(float diff)
        {
            if (owner.IsMovementUpdated())
            {
                sourceBuff.DeactivateBuff();
            }
        }
    }
}

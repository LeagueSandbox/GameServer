using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
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

            _createdParticle = AddParticleTarget(champion, champion, "TeleportHome.troy", champion, lifetime: buff.Duration);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (sourceBuff.TimeElapsed >= sourceBuff.Duration)
            {
                // This is the only case where removing the listener works. Outside of this if statement will crash the server due to the listener being removed before it completes the callback.
                ApiEventManager.OnTakeDamage.RemoveListener(this, ownerSpell.CastInfo.Owner);
                ApiEventManager.OnUnitUpdateMoveOrder.RemoveListener(this, ownerSpell.CastInfo.Owner);
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

        public void OnUpdateMoveOrder(IObjAiBase unit, OrderType order)
        {
            var buff = unit.GetBuffWithName("Recall");
            if (buff != null)
            {
                if (order != OrderType.Hold && order != OrderType.Stop)
                {
                    buff.DeactivateBuff();
                }
                else
                {
                    // After the callback ends, it will remove the listener, so we make a new one before the callback ends.
                    ApiEventManager.OnUnitUpdateMoveOrder.AddListener(this, unit, OnUpdateMoveOrder, true);
                }
            }
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

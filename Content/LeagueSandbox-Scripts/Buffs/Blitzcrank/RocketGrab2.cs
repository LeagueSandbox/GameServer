using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;

namespace RocketGrab2
{
    internal class RocketGrab2 : IBuffGameScript
    {
        public BuffType BuffType => BuffType.STUN;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            unit.Stats.SetActionState(ActionState.CAN_ATTACK, false);
            unit.Stats.SetActionState(ActionState.CAN_CAST, false);
            unit.Stats.SetActionState(ActionState.CAN_MOVE, false);

            ForceMovement(unit, "RUN", buff.SourceUnit.Position, 1800f, 0, 5.0f, 0, movementOrdersType: ForceMovementOrdersType.CANCEL_ORDER);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            unit.Stats.SetActionState(ActionState.CAN_ATTACK, true);
            unit.Stats.SetActionState(ActionState.CAN_CAST, true);
            unit.Stats.SetActionState(ActionState.CAN_MOVE, true);

            if (buff.SourceUnit is IObjAiBase ai && unit is IChampion ch)
            {
                ai.SetTargetUnit(ch, true);
            }
        }

        public void OnUpdate(float diff)
        {
        }
    }
}


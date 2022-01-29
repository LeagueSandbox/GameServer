using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class RocketGrab2 : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            buff.SetStatusEffect(StatusFlags.CanAttack | StatusFlags.CanMove | StatusFlags.CanCast, false);

            ForceMovement(unit, "RUN", buff.SourceUnit.Position, 1800f, 0, 5.0f, 0, movementOrdersType: ForceMovementOrdersType.CANCEL_ORDER);
        }

        // TODO: Use OnMoveEnd event and call this manually.
        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            SetStatus(unit, StatusFlags.CanAttack | StatusFlags.CanMove | StatusFlags.CanCast, true);

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


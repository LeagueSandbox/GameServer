using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServerCore.Logic.Enums;

namespace GameServerCore.Logic.Domain.GameObjects
{
    public interface IObjAiBase : IAttackableUnit
    {
        IAttackableUnit TargetUnit { get; }
        IAttackableUnit AutoAttackTarget { get; }

        float AutoAttackDelay { get; }
        float AutoAttackProjectileSpeed { get; }
        MoveOrder MoveOrder { get; }
        bool IsCastingSpell { get; }
        bool IsMelee { get; }
        void UpdateTargetUnit(IAttackableUnit unit);
    }
}

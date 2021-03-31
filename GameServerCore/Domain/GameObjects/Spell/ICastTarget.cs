using GameServerCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerCore.Domain.GameObjects.Spell
{
    public interface ICastTarget
    {
        IAttackableUnit Unit { get; }
        HitResult HitResult { get; }
    }
}
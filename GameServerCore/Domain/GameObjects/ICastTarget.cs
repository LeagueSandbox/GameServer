using GameServerCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerCore.Domain.GameObjects
{
    public interface ICastTarget
    {
        IAttackableUnit Unit { get; }
        HitResult HitResult { get; }
    }
}
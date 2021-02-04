using System;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    public class CastTarget : ICastTarget
    {
        public IAttackableUnit Unit { get; protected set; }
        public HitResult HitResult { get; protected set; }

        public CastTarget(IAttackableUnit unit, HitResult hitResult)
        {
            Unit = unit;
            HitResult = hitResult;
        }

        public static HitResult GetHitResult(IAttackableUnit unit, ICastInfo castInfo)
        {
            if (castInfo.IsAutoAttack)
            {
                // TODO: Implement Dodge and Miss
                if (castInfo.Owner.IsNextAutoCrit)
                {
                    return HitResult.HIT_Critical;
                }
            }
            return HitResult.HIT_Normal;
        }
    }
}
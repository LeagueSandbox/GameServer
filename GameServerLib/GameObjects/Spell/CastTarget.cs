using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.GameObjects.SpellNS
{
    public class CastTarget
    {
        public AttackableUnit Unit { get; protected set; }
        public HitResult HitResult { get; protected set; }

        public CastTarget(AttackableUnit unit, HitResult hitResult)
        {
            Unit = unit;
            HitResult = hitResult;
        }

        public static HitResult GetHitResult(AttackableUnit unit, bool isAutoAttack, bool isNextAutoCrit)
        {
            if (isAutoAttack)
            {
                // TODO: Implement Dodge and Miss
                if (isNextAutoCrit)
                {
                    return HitResult.HIT_Critical;
                }
            }
            return HitResult.HIT_Normal;
        }
    }
}
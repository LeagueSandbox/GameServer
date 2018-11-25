using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class DamageDoneResponse : ICoreResponse
    {
        public IAttackableUnit Source { get; }
        public IAttackableUnit Target { get; }
        public float Amount { get; }
        public DamageType Type { get; }
        public DamageText DamageText { get; }
        public bool IsGlobal { get; }
        public int SourceId { get; }
        public int TargetId { get; }
        public DamageDoneResponse(IAttackableUnit source, IAttackableUnit target, float amount, DamageType type, DamageText damagetext, bool isGlobal = true, int sourceId = 0, int targetId = 0)
        {
            Source = source;
            Target = target;
            Amount = amount;
            Type = type;
            DamageText = damagetext;
            IsGlobal = isGlobal;
            SourceId = sourceId;
            TargetId = targetId;
        }
    }
};
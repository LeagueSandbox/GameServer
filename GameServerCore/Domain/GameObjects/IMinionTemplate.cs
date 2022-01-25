using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IMinionTemplate
    {
        IObjAiBase Owner { get; }
        string Name { get; }
        string Model { get; }
        Vector2 Position { get; }
        int SkinId { get; }
        TeamId Team { get; }
        uint NetId { get; }
        bool IsTargetable { get; }
        bool IgnoresCollision { get; }
        string AiScript { get; }
        int DamageBonus { get; }
        int HealthBonus { get; }
        int InitialLevel { get; }
        IObjAiBase VisibilityOwner { get; }
    }
}

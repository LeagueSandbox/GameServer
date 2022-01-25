using System.Numerics;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IMonsterTemplate
    {
        string Name { get; set; }
        string Model { get; set; }
        Vector2 Position { get; set; }
        Vector3 FaceDirection { get; set; }
        IMonsterCamp Camp { get; set; }
        TeamId Team { get; set; }
        string SpawnAnimation { get; set; }
        uint NetId { get; set; }
        bool IsTargetable { get; set; }
        bool IgnoresCollision { get; set; }
        string AiScript { get; set; }
        int DamageBonus { get; set; }
        int HealthBonus { get; set; }
        int InitialLevel { get; set; }
    }
}

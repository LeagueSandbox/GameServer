using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Numerics;

namespace GameServerCore.Domain
{
    public interface IFountain
    {
        Vector2 Position { get; }
        TeamId Team { get; }
        void Update(float diff);
    }
}
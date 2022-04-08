using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IFloatingTextData
    {
        IGameObject Target { get; }
        FloatTextType FloatTextType { get; }
        string Message { get; }
        int Param { get; }
    }
}

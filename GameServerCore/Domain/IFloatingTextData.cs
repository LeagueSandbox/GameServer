using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IFloatingTextData
    {
        IChampion Target { get; }
        FloatTextType FloatTextType { get; }
        string Message { get; }
        int Param { get; }
    }
}

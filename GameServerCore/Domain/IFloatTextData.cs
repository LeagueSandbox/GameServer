using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IFloatTextData
    {
        IChampion Target { get; }
        FloatTextType FloatTextType { get; }
        string Message { get; }
        int Param { get; }
    }
}

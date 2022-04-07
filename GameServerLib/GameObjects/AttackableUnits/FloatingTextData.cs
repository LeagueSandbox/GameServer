using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerLib.GameObjects.AttackableUnits
{
    class FloatingTextData : IFloatingTextData
    {
        public IGameObject Target { get; }
        public FloatTextType FloatTextType { get; }
        public string Message { get; }
        public int Param { get; }
        public FloatingTextData(IGameObject target, string message, FloatTextType floatTextType, int param)
        {
            Target = target;
            Message = message;
            FloatTextType = floatTextType;
            Param = param;
        }
    }
}

using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;

namespace GameServerLib.GameObjects.AttackableUnits
{
    public class FloatingTextData
    {
        public GameObject Target { get; }
        public FloatTextType FloatTextType { get; }
        public string Message { get; }
        public int Param { get; }
        public FloatingTextData(GameObject target, string message, FloatTextType floatTextType, int param)
        {
            Target = target;
            Message = message;
            FloatTextType = floatTextType;
            Param = param;
        }
    }
}

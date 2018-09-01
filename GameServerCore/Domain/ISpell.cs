using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface ISpell
    {
        IChampion Owner { get; }
        byte Level { get; }
        byte Slot { get; }
        float CastTime { get; }
        string SpellName { get; }
        SpellState State { get; }
        float CurrentCooldown { get; }
        float CurrentCastTime { get; }
        float CurrentChannelDuration { get; }
        uint FutureProjNetId { get; }
        uint SpellNetId { get; }
        IAttackableUnit Target { get; }
        float X { get; }
        float Y { get; }
        float X2 { get; }
        float Y2 { get; }
        ISpellData SpellData { get; }

        int GetId();
        float GetCooldown();
        void Deactivate();
    }
}

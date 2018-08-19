using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServerCore.Logic.Domain.GameObjects;
using GameServerCore.Logic.Enums;

namespace GameServerCore.Logic.Domain
{
    public interface ISpell
    {
        IChampion Owner { get; }
        short Level { get; }
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
    }
}

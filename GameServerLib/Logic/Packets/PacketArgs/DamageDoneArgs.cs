using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct DamageDoneArgs
    {
        public uint DamageSourceNetId { get; }
        public uint DamageTargetNetId { get; }
        public float Amount { get; }
        public DamageType Type { get; }
        public DamageText DamageText { get; }

        public DamageDoneArgs(uint damageSourceNetId, uint damageTargetNetId, float amount, DamageType type, DamageText damageText)
        {
            DamageSourceNetId = damageSourceNetId;
            DamageTargetNetId = damageTargetNetId;
            Amount = amount;
            Type = type;
            DamageText = damageText;
        }
    }
}

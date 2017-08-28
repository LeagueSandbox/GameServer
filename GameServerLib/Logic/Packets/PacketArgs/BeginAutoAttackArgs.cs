using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct BeginAutoAttackArgs
    {
        public UnitAtLocation Attacker { get; }
        public UnitAtLocation Attacked { get; }
        public uint FutureProjNetId { get; }
        public bool IsCritical { get; }

        public BeginAutoAttackArgs(UnitAtLocation attacker, UnitAtLocation attacked, uint futureProjNetId, bool isCritical)
        {
            Attacker = attacker;
            Attacked = attacked;
            FutureProjNetId = futureProjNetId;
            IsCritical = isCritical;
        }
    }
}

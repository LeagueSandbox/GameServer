using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Surrender : BasePacket
    {
        public Surrender(AttackableUnit starter, byte flag, byte yesVotes, byte noVotes, byte maxVotes, TeamId team, float timeOut)
            : base(PacketCmd.PKT_S2_C_SURRENDER)
        {
            _buffer.Write(flag); // Flag. 2 bits
            _buffer.Write(starter.NetId);
            _buffer.Write(yesVotes);
            _buffer.Write(noVotes);
            _buffer.Write(maxVotes);
            _buffer.Write((int)team);
            _buffer.Write(timeOut);
        }
    }
}
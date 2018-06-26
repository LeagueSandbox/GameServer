using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Surrender : BasePacket
    {
        public Surrender(AttackableUnit starter, byte flag, byte yesVotes, byte noVotes, byte maxVotes, TeamId team, float timeOut)
            : base(PacketCmd.PKT_S2_C_SURRENDER)
        {
            _buffer.Write((byte)flag); // Flag. 2 bits
            _buffer.Write((uint)starter.NetId);
            _buffer.Write((byte)yesVotes);
            _buffer.Write((byte)noVotes);
            _buffer.Write((byte)maxVotes);
            _buffer.Write((int)team);
            _buffer.Write((float)timeOut);
        }
    }
}
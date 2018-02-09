using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Surrender : BasePacket
    {
        public Surrender(AttackableUnit starter, byte flag, byte yesVotes, byte noVotes, byte maxVotes, TeamId team, float timeOut)
            : base(PacketCmd.PKT_S2C_Surrender)
        {
            buffer.Write((byte)flag); // Flag. 2 bits
            buffer.Write((uint)starter.NetId);
            buffer.Write((byte)yesVotes);
            buffer.Write((byte)noVotes);
            buffer.Write((byte)maxVotes);
            buffer.Write((int)team);
            buffer.Write((float)timeOut);
        }
    }
}
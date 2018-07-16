using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Surrender : BasePacket
    {
        public Surrender(Game game, AttackableUnit starter, byte flag, byte yesVotes, byte noVotes, byte maxVotes, TeamId team, float timeOut)
            : base(game, PacketCmd.PKT_S2C_SURRENDER)
        {
            Write(flag); // Flag. 2 bits
            WriteNetId(starter);
            Write(yesVotes);
            Write(noVotes);
            Write(maxVotes);
            Write((int)team);
            Write(timeOut);
        }
    }
}
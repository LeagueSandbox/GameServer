using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetTeam : BasePacket
    {
        public SetTeam(Game game, AttackableUnit unit, TeamId team) : base(game, PacketCmd.PKT_S2C_SET_TEAM)
        {
            WriteNetId(unit);
            Write((int)team);
        }
    }
}
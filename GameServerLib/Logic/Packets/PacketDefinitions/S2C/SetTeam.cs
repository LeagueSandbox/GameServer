using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetTeam : BasePacket
    {
        public SetTeam(AttackableUnit unit, TeamId team) : base(PacketCmd.PKT_S2C_SET_TEAM)
        {
            _buffer.Write(unit.NetId);
            _buffer.Write((int)team);
        }
    }
}
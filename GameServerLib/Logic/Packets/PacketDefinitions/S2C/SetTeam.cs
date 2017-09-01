using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetTeam : BasePacket
    {
        public SetTeam(Unit unit, TeamId team) : base(PacketCmd.PKT_S2C_SetTeam)
        {
            buffer.Write(unit.NetId);
            buffer.Write((int)team);
        }
    }
}
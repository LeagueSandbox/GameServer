using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetCameraPosition : BasePacket
    {
        public SetCameraPosition(Game game, Champion champ, float x, float y, float z)
            : base(game, PacketCmd.PKT_S2C_SET_CAMERA_POSITION, champ.NetId)
        {
            Write(x);
            Write(z); // Doesn't seem to matter
            Write(y);
        }
    }
}
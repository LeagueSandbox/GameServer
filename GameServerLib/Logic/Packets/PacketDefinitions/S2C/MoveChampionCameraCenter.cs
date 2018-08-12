using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MoveChampionCameraCenter : BasePacket
    {
        public MoveChampionCameraCenter(Champion c, bool enable, byte mode, float distance)
            : base(PacketCmd.PKT_S2C_MoveChampionCameraCenter, c.NetId)
        {
            byte state = 0x00;
            if (enable)
            {
                state = 0x01;
            }
            buffer.Write((byte)state);
            buffer.Write((float)distance); // How much it's moved towards
            // where the champion is facing
            // (Can be a negative value; ends up behind the champion)
            buffer.fill(0, 8);
            buffer.Write((byte)mode); // Seems to be a bit field.
            // First bit 1 : Always in front (or back) of the player
            // First bit 0 : Doesn't move when the champion faces another direction
        }
    }
}
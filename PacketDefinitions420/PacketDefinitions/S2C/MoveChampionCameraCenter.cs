using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class MoveChampionCameraCenter : BasePacket
    {
        public MoveChampionCameraCenter(IChampion c, bool enable, byte mode, float distance)
            : base(PacketCmd.PKT_S2C_MOVE_CHAMPION_CAMERA_CENTER, c.NetId)
        {
            byte state = 0x00;
            if (enable)
            {
                state = 0x01;
            }
            Write(state);
            Write(distance); // How much it's moved towards
            // where the champion is facing
            // (Can be a negative value; ends up behind the champion)
            Fill(0, 8);
            Write(mode); // Seems to be a bit field.
            // First bit 1 : Always in front (or back) of the player
            // First bit 0 : Doesn't move when the champion faces another direction
        }
    }
}
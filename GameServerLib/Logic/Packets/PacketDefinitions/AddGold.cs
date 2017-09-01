using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class AddGold : BasePacket
    {

        public AddGold(Champion richMan, Unit died, float gold) : base(PacketCmd.PKT_S2C_AddGold, richMan.NetId)
        {
            buffer.Write(richMan.NetId);
            if (died != null)
            {
                buffer.Write(died.NetId);
            }
            else
            {
                buffer.Write((int)0);
            }
            buffer.Write(gold);
        }
    }
}
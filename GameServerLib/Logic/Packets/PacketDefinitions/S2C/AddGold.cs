using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddGold : BasePacket
    {
        public AddGold(AddGoldArgs args)
            : base(PacketCmd.PKT_S2C_AddGold, args.AcceptorNetId)
        {
            buffer.Write((uint)args.AcceptorNetId);
            buffer.Write((uint)args.DonorNetId);
            buffer.Write((float)args.Amount);
        }
    }
}
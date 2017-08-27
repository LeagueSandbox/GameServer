using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddXP : BasePacket
    {
        public AddXP(AddXpArgs args)
            : base(PacketCmd.PKT_S2C_AddXP)
        {
            buffer.Write(args.AddToNetId);
            buffer.Write(args.Amount);
        }
    }
}
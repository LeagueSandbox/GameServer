using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddUnitFOW : BasePacket
    {
        public AddUnitFOW(AddUnitFOWArgs args)
            : base(PacketCmd.PKT_S2C_AddUnitFOW)
        {
            buffer.Write((uint)args.TargetNetId);
        }
    }
}
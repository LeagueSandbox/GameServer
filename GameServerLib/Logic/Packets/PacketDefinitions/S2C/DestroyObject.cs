using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DestroyObject : BasePacket
    {
        public DestroyObject(DestroyObjectArgs args)
            : base(PacketCmd.PKT_S2C_DestroyObject, args.Destroyer.ObjectNetId)
        {
            buffer.Write((uint)args.Destroyed.ObjectNetId);
        }
    }
}
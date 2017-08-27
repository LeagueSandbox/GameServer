using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Announce : BasePacket
    {
        public Announce(AnnounceArgs args)
            : base(PacketCmd.PKT_S2C_Announce)
        {
            buffer.Write((byte)args.Type);
            buffer.Write((long)0);
            if (args.MapId > 0)
                buffer.Write(args.MapId);
        }
    }
}
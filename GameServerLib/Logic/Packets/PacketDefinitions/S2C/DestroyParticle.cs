using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DestroyParticle : BasePacket
    {
        public DestroyParticle(PacketObject args)
            : base(PacketCmd.PKT_S2C_DestroyObject, args.ObjectNetId)
        {
            buffer.Write((uint)args.ObjectNetId);
        }
    }
}
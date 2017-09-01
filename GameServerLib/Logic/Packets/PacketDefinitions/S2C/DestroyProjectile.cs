using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DestroyProjectile : BasePacket
    {
        public DestroyProjectile(PacketObject args)
            : base(PacketCmd.PKT_S2C_DestroyProjectile, args.ObjectNetId)
        {

        }
    }
}
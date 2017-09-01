using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DisconnectedAnnouncement : BasePacket
    {
        public DisconnectedAnnouncement(PacketObject args) 
            : base(PacketCmd.PKT_S2C_DisconnectedAnnouncement)
        {
            buffer.Write(args.ObjectNetId); //unit net id
        }
    }
}
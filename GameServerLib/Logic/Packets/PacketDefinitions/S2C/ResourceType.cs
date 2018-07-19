using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ResourceType : BasePacket
    {
        public ResourceType(Game game, uint playernetid, byte resourceType)
            : base(game, PacketCmd.PKT_S2C_RESOURCE_TYPE, playernetid)
        {
            Write(resourceType);
        }
    }
}
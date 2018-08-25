using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ResourceType : BasePacket
    {
        public ResourceType(uint playernetid, byte resourceType)
            : base(PacketCmd.PKT_S2C_RESOURCE_TYPE, playernetid)
        {
            Write(resourceType);
        }
    }
}
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ResourceType : BasePacket
    {
        public ResourceType(uint playernetid, byte resourceType)
            : base(PacketCmd.PKT_S2_C_RESOURCE_TYPE, playernetid)
        {
            _buffer.Write((byte)resourceType);
        }
    }
}
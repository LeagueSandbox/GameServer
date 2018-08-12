using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SoundSettings : BasePacket
    {
        public SoundSettings(byte soundCategory, bool mute) 
            : base(PacketCmd.PKT_S2C_SoundSettings)
        {
            buffer.Write((byte)soundCategory);
            buffer.Write(mute);
        }
    }
}
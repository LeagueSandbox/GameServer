using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SoundSettings : BasePacket
    {
        public SoundSettings(byte soundCategory, bool mute)
            : base(PacketCmd.PKT_S2C_SOUND_SETTINGS)
        {
            Write(soundCategory);
            Write(mute);
        }
    }
}
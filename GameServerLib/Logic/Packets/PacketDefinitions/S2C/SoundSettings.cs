using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SoundSettings : BasePacket
    {
        public SoundSettings(byte soundCategory, bool mute) 
            : base(PacketCmd.PKT_S2_C_SOUND_SETTINGS)
        {
            _buffer.Write(soundCategory);
            _buffer.Write(mute);
        }
    }
}
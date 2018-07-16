using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SoundSettings : BasePacket
    {
        public SoundSettings(Game game, byte soundCategory, bool mute)
            : base(game, PacketCmd.PKT_S2C_SOUND_SETTINGS)
        {
            Write(soundCategory);
            Write(mute);
        }
    }
}
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ChangeCharacterVoice : BasePacket
    {
        public ChangeCharacterVoice(uint netId, string voiceOverride, bool resetOverride = false)
            : base(PacketCmd.PKT_S2C_CHANGE_CHARACTER_VOICE, netId)
        {
            Write(resetOverride); // If this is 1, resets voice to default state and ignores voiceOverride
            WriteConstLengthString(voiceOverride, 32, true);
        }
    }
}
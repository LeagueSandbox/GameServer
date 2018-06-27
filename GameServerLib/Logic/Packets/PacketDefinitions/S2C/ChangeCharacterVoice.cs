using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChangeCharacterVoice : BasePacket
    {
        public ChangeCharacterVoice(uint netId, string voiceOverride, bool resetOverride = false)
            : base(PacketCmd.PKT_S2_C_CHANGE_CHARACTER_VOICE, netId)
        {
            _buffer.Write(resetOverride); // If this is 1, resets voice to default state and ignores voiceOverride
            foreach (var b in Encoding.Default.GetBytes(voiceOverride))
                _buffer.Write(b);
            if (voiceOverride.Length < 32)
                _buffer.Fill(0, 32 - voiceOverride.Length);
        }
    }
}
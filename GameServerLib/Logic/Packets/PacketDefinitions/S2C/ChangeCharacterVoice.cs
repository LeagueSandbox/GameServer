using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChangeCharacterVoice : BasePacket
    {
        public ChangeCharacterVoice(uint netID, string voiceOverride, bool resetOverride = false)
            : base(PacketCmd.PKT_S2C_ChangeCharacterVoice, netID)
        {
            buffer.Write(resetOverride); // If this is 1, resets voice to default state and ignores voiceOverride
            foreach (var b in Encoding.Default.GetBytes(voiceOverride))
                buffer.Write((byte)b);
            if (voiceOverride.Length < 32)
                buffer.fill(0, 32 - voiceOverride.Length);
        }
    }
}
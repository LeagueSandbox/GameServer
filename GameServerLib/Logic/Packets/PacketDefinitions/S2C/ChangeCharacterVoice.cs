using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChangeCharacterVoice : BasePacket
    {
        public ChangeCharacterVoice(Game game, uint netId, string voiceOverride, bool resetOverride = false)
            : base(game, PacketCmd.PKT_S2C_CHANGE_CHARACTER_VOICE, netId)
        {
            Write(resetOverride); // If this is 1, resets voice to default state and ignores voiceOverride
            WriteConstLengthString(voiceOverride, 32, true);
        }
    }
}
using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class UpdateModel : BasePacket
    {
        public UpdateModel(uint netId, string modelName, bool useSpells = true)
            : base(PacketCmd.PKT_S2_C_UPDATE_MODEL, netId)
        {
            _buffer.Write(useSpells); // Use spells from the new model
            _buffer.Write((byte)0x00); // <-- These three bytes most likely form
            _buffer.Write((byte)0x00); // <-- an int with the useSpells byte, but
            _buffer.Write((byte)0x00); // <-- they don't seem to affect anything
            _buffer.Write((byte)1); // Bit field with bits 1 and 2. Unk
            _buffer.Write((int)-1); // SkinID (Maybe -1 means keep using current one?)
            foreach (var b in Encoding.Default.GetBytes(modelName))
                _buffer.Write((byte)b);
            if (modelName.Length < 32)
                _buffer.Fill(0, 32 - modelName.Length);
        }
    }
}
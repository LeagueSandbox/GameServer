using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class UpdateModel : BasePacket
    {
        public UpdateModel(uint netID, string modelName, bool useSpells = true)
            : base(PacketCmd.PKT_S2C_UpdateModel, netID)
        {
            buffer.Write(useSpells); // Use spells from the new model
            buffer.Write((byte)0x00); // <-- These three bytes most likely form
            buffer.Write((byte)0x00); // <-- an int with the useSpells byte, but
            buffer.Write((byte)0x00); // <-- they don't seem to affect anything
            buffer.Write((byte)1); // Bit field with bits 1 and 2. Unk
            buffer.Write((int)-1); // SkinID (Maybe -1 means keep using current one?)
            foreach (var b in Encoding.Default.GetBytes(modelName))
                buffer.Write((byte)b);
            if (modelName.Length < 32)
                buffer.fill(0, 32 - modelName.Length);
        }
    }
}
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class UpdateModel : BasePacket
    {
        public UpdateModel(uint netId, string modelName, bool useSpells = true)
            : base(PacketCmd.PKT_S2C_UPDATE_MODEL, netId)
        {
            Write(useSpells); // Use spells from the new model
            Write((byte)0x00); // <-- These three bytes most likely form
            Write((byte)0x00); // <-- an int with the useSpells byte, but
            Write((byte)0x00); // <-- they don't seem to affect anything
            Write((byte)1); // Bit field with bits 1 and 2. Unk
            Write(-1); // SkinID (Maybe -1 means keep using current one?)
			WriteConstLengthString(modelName, 32, true);
        }
    }
}
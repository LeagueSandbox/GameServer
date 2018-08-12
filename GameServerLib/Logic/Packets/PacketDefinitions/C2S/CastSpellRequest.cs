using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class CastSpellRequest
    {
        public PacketCmd cmd;
        public int netId;
        public byte spellSlotType; // 4.18 [deprecated? . 2 first(highest) bits: 10 - ability or item, 01 - summoner spell]
        public byte spellSlot; // 0-3 [0-1 if spellSlotType has summoner spell bits set]
        public float x; // Initial point
        public float y; // (e.g. Viktor E laser starting point)
        public float x2; // Final point
        public float y2; // (e.g. Viktor E laser final point)
        public uint targetNetId; // If 0, use coordinates, else use target net id

        public CastSpellRequest(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            spellSlotType = reader.ReadByte();
            spellSlot = reader.ReadByte();
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            x2 = reader.ReadSingle();
            y2 = reader.ReadSingle();
            targetNetId = reader.ReadUInt32();
        }
    }
}
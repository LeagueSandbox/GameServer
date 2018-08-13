using System.IO;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class CastSpellRequest
    {
        public PacketCmd Cmd;
        public int NetId;
        public byte SpellSlotType; // 4.18 [deprecated? . 2 first(highest) bits: 10 - ability or item, 01 - summoner spell]
        public byte SpellSlot; // 0-3 [0-1 if spellSlotType has summoner spell bits set]
        public float X; // Initial point
        public float Y; // (e.g. Viktor E laser starting point)
        public float X2; // Final point
        public float Y2; // (e.g. Viktor E laser final point)
        public uint TargetNetId; // If 0, use coordinates, else use target net id

        public CastSpellRequest(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                NetId = reader.ReadInt32();
                SpellSlotType = reader.ReadByte();
                SpellSlot = reader.ReadByte();
                X = reader.ReadSingle();
                Y = reader.ReadSingle();
                X2 = reader.ReadSingle();
                Y2 = reader.ReadSingle();
                TargetNetId = reader.ReadUInt32();
            }
        }
    }
}
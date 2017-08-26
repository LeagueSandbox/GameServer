using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class CastSpellRequest : ClientPacketBase
    {
        public byte SpellSlotType { get; private set; } // 4.18 [deprecated? . 2 first(highest) bits: 10 - ability or item, 01 - summoner spell]
        public byte SpellSlot { get; private set; } // 0-3 [0-1 if spellSlotType has summoner spell bits set]
        public float X { get; private set; } // Initial point
        public float Y { get; private set; } // (e.g. Viktor E laser starting point)
        public float X2 { get; private set; } // Final point
        public float Y2 { get; private set; } // (e.g. Viktor E laser final point)
        public uint TargetNetId { get; private set; } // If 0, use coordinates, else use target net id

        public CastSpellRequest(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
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
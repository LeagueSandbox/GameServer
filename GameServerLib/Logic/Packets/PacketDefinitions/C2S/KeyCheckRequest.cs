using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class KeyCheckRequest : ClientPacketBase
    {
        public byte[] PartialKey { get; private set; } = new byte[3];   //Bytes 1 to 3 from the blowfish key for that client
        public uint PlayerNo { get; private set; }
        public long UserId { get; private set; }        //short testVar[8];   //User id
        public uint Trash { get; private set; }
        public ulong CheckId { get; private set; }        //short checkVar[8];  //Encrypted testVar
        public uint Trash2 { get; private set; }

        public KeyCheckRequest(byte[] bytes) : base(bytes)
        {

        }

        public override void Parse()
        {
            using (var stream = new MemoryStream(OriginalData))
            using (var reader = new BinaryReader(stream))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                ParseInternal(reader);
            }
        }

        protected override void ParseInternal(BinaryReader reader)
        {
            PartialKey[0] = reader.ReadByte();
            PartialKey[1] = reader.ReadByte();
            PartialKey[2] = reader.ReadByte();
            PlayerNo = reader.ReadUInt32();
            UserId = reader.ReadInt64();
            Trash = reader.ReadUInt32();
            CheckId = reader.ReadUInt64();
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                Trash2 = reader.ReadUInt32();
            }
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class ChatMessage : ClientPacketBase
    {
        public int BotNetId { get; private set; }
        public byte IsBotMessage { get; private set; }
        public ChatType Type { get; private set; }
        public int Unk1 { get; private set; } // playerNo?
        public int Length { get; private set; }
        public byte[] Unk2 { get; private set; } = new byte[32];
        public string Msg { get; private set; }

        public ChatMessage(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            BotNetId = reader.ReadInt32();
            IsBotMessage = reader.ReadByte();
            Type = (ChatType)reader.ReadInt32();
            Unk1 = reader.ReadInt32();
            Length = reader.ReadInt32();
            Unk2 = reader.ReadBytes(32);

            var bytes = new List<byte>();
            for (var i = 0; i < Length; i++)
                bytes.Add(reader.ReadByte());
            Msg = Encoding.Default.GetString(bytes.ToArray());
        }
    }
}
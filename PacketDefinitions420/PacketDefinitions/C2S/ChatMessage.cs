using System.Collections.Generic;
using System.IO;
using System.Text;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class ChatMessage
    {
        public PacketCmd Cmd;
        public int PlayerId;
        public int BotNetId;
        public byte IsBotMessage;

        public ChatType Type;
        public int Unk1; // playerNo?
        public int Length;
        public byte[] Unk2 = new byte[32];
        public string Msg;

        public ChatMessage(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                PlayerId = reader.ReadInt32();
                BotNetId = reader.ReadInt32();
                IsBotMessage = reader.ReadByte();
                Type = (ChatType)reader.ReadInt32();
                Unk1 = reader.ReadInt32();
                Length = reader.ReadInt32();
                Unk2 = reader.ReadBytes(32);

                var bytes = new List<byte>();
                for (var i = 0; i < Length; i++)
                {
                    bytes.Add(reader.ReadByte());
                }

                Msg = Encoding.Default.GetString(bytes.ToArray());
            }
        }
    }
}
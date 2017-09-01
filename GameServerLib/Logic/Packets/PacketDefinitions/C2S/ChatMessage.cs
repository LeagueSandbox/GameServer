using System.Collections.Generic;
using System.IO;
using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class ChatMessage
    {
        public PacketCmd cmd;
        public int playerId;
        public int botNetId;
        public byte isBotMessage;

        public ChatType type;
        public int unk1; // playerNo?
        public int length;
        public byte[] unk2 = new byte[32];
        public string msg;

        public ChatMessage(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            playerId = reader.ReadInt32();
            botNetId = reader.ReadInt32();
            isBotMessage = reader.ReadByte();
            type = (ChatType)reader.ReadInt32();
            unk1 = reader.ReadInt32();
            length = reader.ReadInt32();
            unk2 = reader.ReadBytes(32);

            var bytes = new List<byte>();
            for (var i = 0; i < length; i++)
                bytes.Add(reader.ReadByte());
            msg = Encoding.Default.GetString(bytes.ToArray());
        }
    }
}
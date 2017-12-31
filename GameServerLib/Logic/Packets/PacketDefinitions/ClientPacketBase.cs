using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions
{
    public abstract class ClientPacketBase
    {
        public readonly byte[] OriginalData;

        public PacketCmd Cmd { get; protected set; }
        public uint NetId { get; protected set; }

        protected ClientPacketBase(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            OriginalData = data;
            Parse();
        }

        public virtual void Parse()
        {
            using (var stream = new MemoryStream(OriginalData))
            using (var reader = new BinaryReader(stream))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                NetId = reader.ReadUInt32();
                ParseInternal(reader);
            }
        }

        protected abstract void ParseInternal(BinaryReader reader);
    }
}

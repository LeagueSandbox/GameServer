using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    /// <summary> Represents an packet which is either not used or its structure is not known yet</summary>
    public class EmptyClientPacket : ClientPacketBase
    {
        public EmptyClientPacket(byte[] data) : base(data)
        {
        }

        public override void Parse()
        {
            using (var stream = new MemoryStream(OriginalData))
            using (var reader = new BinaryReader(stream))
            {
                Cmd = (PacketCmd)reader.ReadByte();
            }
        }

        protected override void ParseInternal(BinaryReader reader)
        {

        }
    }
}

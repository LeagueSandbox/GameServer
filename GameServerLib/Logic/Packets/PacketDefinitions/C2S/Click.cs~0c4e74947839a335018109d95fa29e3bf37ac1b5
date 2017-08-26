using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class Click : ClientPacketBase
    {
        public int Zero { get; private set; }
        public uint TargetNetId { get; private set; } // netId on which the player clicked

        public Click(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            Zero = reader.ReadInt32();
            TargetNetId = reader.ReadUInt32();
        }
    }
}
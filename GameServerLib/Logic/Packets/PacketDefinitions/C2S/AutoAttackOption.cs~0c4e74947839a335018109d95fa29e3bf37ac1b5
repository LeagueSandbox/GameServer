using System.IO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class AutoAttackOption : ClientPacketBase
    {
        public byte Activated { get; private set; }

        public AutoAttackOption(byte[] data) : base(data)
        {
        }

        protected override void ParseInternal(BinaryReader reader)
        {
            Activated = reader.ReadByte();
        }
    }
}
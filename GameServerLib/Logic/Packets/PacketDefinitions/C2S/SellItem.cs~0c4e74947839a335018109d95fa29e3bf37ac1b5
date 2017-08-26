using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class SellItem : ClientPacketBase
    {
        public byte SlotId { get; private set; }

        public SellItem(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            SlotId = reader.ReadByte();
        }
    }
}
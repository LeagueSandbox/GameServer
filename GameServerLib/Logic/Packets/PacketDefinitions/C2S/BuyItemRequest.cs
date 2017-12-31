using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class BuyItemRequest : ClientPacketBase
    {
        public int ItemId { get; private set; }

        public BuyItemRequest(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            ItemId = reader.ReadInt32();
        }
    }
}
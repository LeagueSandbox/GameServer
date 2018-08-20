﻿namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SellItemRequest
    {
        public int NetId { get; }
        public byte SlotId { get; }

        public SellItemRequest(int netId, byte slotId)
        {
            NetId = netId;
            SlotId = slotId;
        }
    }
}

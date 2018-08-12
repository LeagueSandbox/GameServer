using System;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class TeleportRequest : BasePacket
    {
        static short a = 0x01;
        public TeleportRequest(uint netId, float x, float y, bool first) 
            : base(PacketCmd.PKT_S2C_MoveAns)
        {
            buffer.Write((int)Environment.TickCount); // syncID
            buffer.Write((byte)0x01); // Unk
            buffer.Write((byte)0x00); // Unk
            if (first == true) //seems to be id, 02 = before teleporting, 03 = do teleport
                buffer.Write((byte)0x02);
            else
                buffer.Write((byte)0x03);
            buffer.Write((int)netId);
            if (first == false)
            {
                buffer.Write((byte)a); // if it is the second part, send 0x01 before coords
                a++;
            }
            buffer.Write((short)x);
            buffer.Write((short)y);
        }

    }
}
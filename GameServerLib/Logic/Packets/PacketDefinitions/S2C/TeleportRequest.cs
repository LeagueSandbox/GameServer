using System;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class TeleportRequest : BasePacket
    {
        static short _a = 0x01;
        public TeleportRequest(uint netId, float x, float y, bool first) 
            : base(PacketCmd.PKT_S2_C_MOVE_ANS)
        {
            _buffer.Write((int)Environment.TickCount); // syncID
            _buffer.Write((byte)0x01); // Unk
            _buffer.Write((byte)0x00); // Unk
            if (first == true) //seems to be id, 02 = before teleporting, 03 = do teleport
                _buffer.Write((byte)0x02);
            else
                _buffer.Write((byte)0x03);
            _buffer.Write((int)netId);
            if (first == false)
            {
                _buffer.Write((byte)_a); // if it is the second part, send 0x01 before coords
                _a++;
            }
            _buffer.Write((short)x);
            _buffer.Write((short)y);
        }

    }
}
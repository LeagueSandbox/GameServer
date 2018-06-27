using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MinionSpawn : BasePacket
    {
        public MinionSpawn(Minion m)
            : base(PacketCmd.PKT_S2_C_OBJECT_SPAWN, m.NetId)
        {
            _buffer.Write((uint)0x00150017); // unk
            _buffer.Write((byte)0x03); // SpawnType - 3 = minion
            _buffer.Write(m.NetId);
            _buffer.Write(m.NetId);
            _buffer.Write((uint)m.SpawnPosition);
            _buffer.Write((byte)0xFF); // unk
            _buffer.Write((byte)1); // wave number ?

            _buffer.Write((byte)m.GetType());

            if (m.GetType() == MinionSpawnType.MINION_TYPE_MELEE)
            {
                _buffer.Write((byte)0); // unk
            }
            else
            {
                _buffer.Write((byte)1); // unk
            }

            _buffer.Write((byte)0); // unk

            if (m.GetType() == MinionSpawnType.MINION_TYPE_CASTER)
            {
                _buffer.Write(0x00010007); // unk
            }
            else if (m.GetType() == MinionSpawnType.MINION_TYPE_MELEE)
            {
                _buffer.Write(0x0001000A); // unk
            }
            else if (m.GetType() == MinionSpawnType.MINION_TYPE_CANNON)
            {
                _buffer.Write(0x0001000D);
            }
            else
            {
                _buffer.Write(0x00010007); // unk
            }
            _buffer.Write(0x00000000); // unk
            _buffer.Write(0x00000000); // unk
            _buffer.Write((short)0x0000); // unk
            _buffer.Write(1.0f); // unk
            _buffer.Write(0x00000000); // unk
            _buffer.Write(0x00000000); // unk
            _buffer.Write(0x00000000); // unk
            _buffer.Write((short)0x0200); // unk
            _buffer.Write(Environment.TickCount); // unk

            var waypoints = m.Waypoints;

            _buffer.Write((byte)((waypoints.Count - m.CurWaypoint + 1) * 2)); // coordCount
            _buffer.Write((int)m.NetId);
            _buffer.Write((byte)0); // movement mask
            _buffer.Write(MovementVector.TargetXToNormalFormat(m.X));
            _buffer.Write(MovementVector.TargetYToNormalFormat(m.Y));
            for (var i = m.CurWaypoint; i < waypoints.Count; ++i)
            {
                _buffer.Write(MovementVector.TargetXToNormalFormat(waypoints[i].X));
                _buffer.Write(MovementVector.TargetXToNormalFormat(waypoints[i].Y));
            }
        }
    }
}
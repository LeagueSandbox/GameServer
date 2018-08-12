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
            : base(PacketCmd.PKT_S2C_ObjectSpawn, m.NetId)
        {
            buffer.Write((uint)0x00150017); // unk
            buffer.Write((byte)0x03); // SpawnType - 3 = minion
            buffer.Write((uint)m.NetId);
            buffer.Write((uint)m.NetId);
            buffer.Write((uint)m.SpawnPosition);
            buffer.Write((byte)0xFF); // unk
            buffer.Write((byte)1); // wave number ?

            buffer.Write((byte)m.getType());

            if (m.getType() == MinionSpawnType.MINION_TYPE_MELEE)
            {
                buffer.Write((byte)0); // unk
            }
            else
            {
                buffer.Write((byte)1); // unk
            }

            buffer.Write((byte)0); // unk

            if (m.getType() == MinionSpawnType.MINION_TYPE_CASTER)
            {
                buffer.Write((int)0x00010007); // unk
            }
            else if (m.getType() == MinionSpawnType.MINION_TYPE_MELEE)
            {
                buffer.Write((int)0x0001000A); // unk
            }
            else if (m.getType() == MinionSpawnType.MINION_TYPE_CANNON)
            {
                buffer.Write((int)0x0001000D);
            }
            else
            {
                buffer.Write((int)0x00010007); // unk
            }
            buffer.Write((int)0x00000000); // unk
            buffer.Write((int)0x00000000); // unk
            buffer.Write((short)0x0000); // unk
            buffer.Write((float)1.0f); // unk
            buffer.Write((int)0x00000000); // unk
            buffer.Write((int)0x00000000); // unk
            buffer.Write((int)0x00000000); // unk
            buffer.Write((short)0x0200); // unk
            buffer.Write((int)Environment.TickCount); // unk

            List<Vector2> waypoints = m.Waypoints;

            buffer.Write((byte)((waypoints.Count - m.CurWaypoint + 1) * 2)); // coordCount
            buffer.Write((int)m.NetId);
            buffer.Write((byte)0); // movement mask
            buffer.Write((short)MovementVector.TargetXToNormalFormat(m.X));
            buffer.Write((short)MovementVector.TargetYToNormalFormat(m.Y));
            for (int i = m.CurWaypoint; i < waypoints.Count; ++i)
            {
                buffer.Write((short)MovementVector.TargetXToNormalFormat(waypoints[i].X));
                buffer.Write((short)MovementVector.TargetXToNormalFormat(waypoints[i].Y));
            }
        }
    }
}
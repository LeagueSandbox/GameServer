using System;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class LaneMinionSpawn : BasePacket
    {
        public LaneMinionSpawn(INavGrid navGrid, ILaneMinion m)
            : base(PacketCmd.PKT_S2C_OBJECT_SPAWN, m.NetId)
        {
            Write((uint)0x00150017); // unk
            Write((byte)0x03); // SpawnType - 3 = minion
            WriteNetId(m);
            WriteNetId(m);
            Write((uint)m.SpawnPosition);
            Write((byte)0xFF); // unk
            Write((byte)1); // wave number ?

            Write((byte)m.MinionSpawnType);

            if (m.MinionSpawnType == MinionSpawnType.MINION_TYPE_MELEE)
            {
                Write((byte)0); // unk
            }
            else
            {
                Write((byte)1); // unk
            }

            Write((byte)0); // unk

            if (m.MinionSpawnType == MinionSpawnType.MINION_TYPE_CASTER)
            {
                Write(0x00010007); // unk
            }
            else if (m.MinionSpawnType == MinionSpawnType.MINION_TYPE_MELEE)
            {
                Write(0x0001000A); // unk
            }
            else if (m.MinionSpawnType == MinionSpawnType.MINION_TYPE_CANNON)
            {
                Write(0x0001000D);
            }
            else
            {
                Write(0x00010007); // unk
            }
            Write(0x00000000); // unk
            Write(0x00000000); // unk
            Write((short)0x0000); // unk
            Write(1.0f); // unk
            Write(0x00000000); // unk
            Write(0x00000000); // unk
            Write(0x00000000); // unk
            Write((short)0x0200); // unk
            Write(Environment.TickCount); // unk

            var waypoints = m.Waypoints;

            Write((byte)((waypoints.Count - m.CurWaypoint + 1) * 2)); // coordCount
            WriteNetId(m);
            Write((byte)0); // movement mask
            Write(MovementVector.TargetXToNormalFormat(navGrid, m.X));
            Write(MovementVector.TargetYToNormalFormat(navGrid, m.Y));
            for (var i = m.CurWaypoint; i < waypoints.Count; ++i)
            {
                Write(MovementVector.TargetXToNormalFormat(navGrid, waypoints[i].X));
                Write(MovementVector.TargetXToNormalFormat(navGrid, waypoints[i].Y));
            }
        }
    }
}
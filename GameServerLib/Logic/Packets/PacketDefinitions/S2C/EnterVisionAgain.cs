using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    /// <summary>
    /// This is basically a "Unit Spawn" packet with only the net ID and the additionnal data
    /// </summary>
    public class EnterVisionAgain : BasePacket
    {
        public EnterVisionAgain(EnterVisionAgainArgs m)
            : base(PacketCmd.PKT_S2C_ObjectSpawn, m.Object.UnitNetId)
        {
            buffer.fill(0, 13);
            buffer.Write(1.0f);
            buffer.fill(0, 13);
            buffer.Write((byte)0x02);
            buffer.Write((int)Environment.TickCount); // unk

            var waypoints = m.Waypoints;

            buffer.Write((byte)((waypoints.Count - m.CurrentWaypoint + 1) * 2)); // coordCount
            buffer.Write((int)m.Object.UnitNetId);
            // TODO: Check if Movement.EncodeWaypoints is what we need to use here
            buffer.Write((byte)0); // movement mask
            buffer.Write((short)MovementVector.TargetXToNormalFormat(m.Object.X));
            buffer.Write((short)MovementVector.TargetYToNormalFormat(m.Object.Y));
            for (int i = m.CurrentWaypoint; i < waypoints.Count; i++)
            {
                buffer.Write(MovementVector.TargetXToNormalFormat((float)waypoints[i].X));
                buffer.Write(MovementVector.TargetXToNormalFormat((float)waypoints[i].Y));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    /// <summary>
    /// This is basically a "Unit Spawn" packet with only the net ID and the additionnal data
    /// </summary>
    public class EnterVisionAgain : BasePacket
    {
        public EnterVisionAgain(Minion m) 
            : base(PacketCmd.PKT_S2C_ObjectSpawn, m.NetId)
        {
            buffer.fill(0, 13);
            buffer.Write(1.0f);
            buffer.fill(0, 13);
            buffer.Write((byte)0x02);
            buffer.Write((int)Environment.TickCount); // unk

            var waypoints = m.Waypoints;

            buffer.Write((byte)((waypoints.Count - m.CurWaypoint + 1) * 2)); // coordCount
            buffer.Write((int)m.NetId);
            // TODO: Check if Movement.EncodeWaypoints is what we need to use here
            buffer.Write((byte)0); // movement mask
            buffer.Write((short)MovementVector.TargetXToNormalFormat(m.X));
            buffer.Write((short)MovementVector.TargetYToNormalFormat(m.Y));
            for (int i = m.CurWaypoint; i < waypoints.Count; i++)
            {
                buffer.Write(MovementVector.TargetXToNormalFormat((float)waypoints[i].X));
                buffer.Write(MovementVector.TargetXToNormalFormat((float)waypoints[i].Y));
            }
        }

        public EnterVisionAgain(Champion c) : base(PacketCmd.PKT_S2C_ObjectSpawn, c.NetId)
        {
            buffer.Write((short)0); // extraInfo
            buffer.Write((byte)0); //c.getInventory().getItems().size(); // itemCount?
            //buffer.Write((short)7; // unknown

            /*
            for (int i = 0; i < c.getInventory().getItems().size(); i++) {
               ItemInstance* item = c.getInventory().getItems()[i];

               if (item != 0 && item.getTemplate() != 0) {
                  buffer.Write((short)item.getStacks();
                  buffer.Write((short)0; // unk
                  buffer.Write((int)item.getTemplate().getId();
                  buffer.Write((short)item.getSlot();
               }
               else {
                  buffer.fill(0, 7);
               }
            }
            */

            buffer.fill(0, 10);
            buffer.Write((float)1.0f);
            buffer.fill(0, 13);

            buffer.Write((byte)2); // Type of data: Waypoints=2
            buffer.Write((int)Environment.TickCount); // unk

            List<Vector2> waypoints = c.Waypoints;

            buffer.Write((byte)((waypoints.Count - c.CurWaypoint + 1) * 2)); // coordCount
            buffer.Write(c.NetId);
            buffer.Write((byte)0); // movement mask; 1=KeepMoving?
            buffer.Write(MovementVector.TargetXToNormalFormat(c.X));
            buffer.Write(MovementVector.TargetYToNormalFormat(c.Y));
            for (int i = c.CurWaypoint; i < waypoints.Count; ++i)
            {
                buffer.Write(MovementVector.TargetXToNormalFormat(waypoints[i].X));
                buffer.Write(MovementVector.TargetXToNormalFormat(waypoints[i].Y));
            }
        }
    }
}
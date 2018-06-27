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
            : base(PacketCmd.PKT_S2_C_OBJECT_SPAWN, m.NetId)
        {
            _buffer.Fill(0, 13);
            _buffer.Write(1.0f);
            _buffer.Fill(0, 13);
            _buffer.Write((byte)0x02);
            _buffer.Write(Environment.TickCount); // unk

            var waypoints = m.Waypoints;

            _buffer.Write((byte)((waypoints.Count - m.CurWaypoint + 1) * 2)); // coordCount
            _buffer.Write((int)m.NetId);
            // TODO: Check if Movement.EncodeWaypoints is what we need to use here
            _buffer.Write((byte)0); // movement mask
            _buffer.Write(MovementVector.TargetXToNormalFormat(m.X));
            _buffer.Write(MovementVector.TargetYToNormalFormat(m.Y));
            for (int i = m.CurWaypoint; i < waypoints.Count; i++)
            {
                _buffer.Write(MovementVector.TargetXToNormalFormat(waypoints[i].X));
                _buffer.Write(MovementVector.TargetXToNormalFormat(waypoints[i].Y));
            }
        }

        public EnterVisionAgain(Champion c) : base(PacketCmd.PKT_S2_C_OBJECT_SPAWN, c.NetId)
        {
            _buffer.Write((short)0); // extraInfo
            _buffer.Write((byte)0); //c.getInventory().getItems().size(); // itemCount?
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

            _buffer.Fill(0, 10);
            _buffer.Write(1.0f);
            _buffer.Fill(0, 13);

            _buffer.Write((byte)2); // Type of data: Waypoints=2
            _buffer.Write(Environment.TickCount); // unk

            List<Vector2> waypoints = c.Waypoints;

            _buffer.Write((byte)((waypoints.Count - c.CurWaypoint + 1) * 2)); // coordCount
            _buffer.Write(c.NetId);
            _buffer.Write((byte)0); // movement mask; 1=KeepMoving?
            _buffer.Write(MovementVector.TargetXToNormalFormat(c.X));
            _buffer.Write(MovementVector.TargetYToNormalFormat(c.Y));
            for (int i = c.CurWaypoint; i < waypoints.Count; ++i)
            {
                _buffer.Write(MovementVector.TargetXToNormalFormat(waypoints[i].X));
                _buffer.Write(MovementVector.TargetXToNormalFormat(waypoints[i].Y));
            }
        }
    }
}
using System;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    /// <summary>
    /// This is basically a "Unit Spawn" packet with only the net ID and the additionnal data
    /// </summary>
    public class EnterVisionAgain : BasePacket
    {
        public EnterVisionAgain(INavGrid navGrid, IMinion m)
            : base(PacketCmd.PKT_S2C_OBJECT_SPAWN, m.NetId)
        {
            Fill(0, 13);
            Write(1.0f);
            Fill(0, 13);
            Write((byte)0x02);
            Write(Environment.TickCount); // unk

            var waypoints = m.Waypoints;

            Write((byte)((waypoints.Count - m.CurWaypoint + 1) * 2)); // coordCount
            WriteNetId(m);
            // TODO: Check if Movement.EncodeWaypoints is what we need to use here
            Write((byte)0); // movement mask
            Write(MovementVector.TargetXToNormalFormat(navGrid, m.X));
            Write(MovementVector.TargetYToNormalFormat(navGrid, m.Y));
            for (var i = m.CurWaypoint; i < waypoints.Count; i++)
            {
                Write(MovementVector.TargetXToNormalFormat(navGrid, waypoints[i].X));
                Write(MovementVector.TargetXToNormalFormat(navGrid, waypoints[i].Y));
            }
        }

        public EnterVisionAgain(INavGrid navGrid, IChampion c)
            : base(PacketCmd.PKT_S2C_OBJECT_SPAWN, c.NetId)
        {
            Write((short)0); // extraInfo
            Write((byte)0); //c.getInventory().getItems().size(); // itemCount?
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

            Fill(0, 10);
            Write(1.0f);
            Fill(0, 13);

            Write((byte)2); // Type of data: Waypoints=2
            Write(Environment.TickCount); // unk

            var waypoints = c.Waypoints;

            Write((byte)((waypoints.Count - c.CurWaypoint + 1) * 2)); // coordCount
            WriteNetId(c);
            Write((byte)0); // movement mask; 1=KeepMoving?
            Write(MovementVector.TargetXToNormalFormat(navGrid, c.X));
            Write(MovementVector.TargetYToNormalFormat(navGrid, c.Y));
            for (var i = c.CurWaypoint; i < waypoints.Count; ++i)
            {
                Write(MovementVector.TargetXToNormalFormat(navGrid, waypoints[i].X));
                Write(MovementVector.TargetXToNormalFormat(navGrid, waypoints[i].Y));
            }
        }
    }
}
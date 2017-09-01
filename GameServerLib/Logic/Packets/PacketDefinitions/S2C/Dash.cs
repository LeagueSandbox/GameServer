using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Dash : BasePacket
    {
        public Dash(Unit u,
            Target t,
            float dashSpeed,
            bool keepFacingLastDirection,
            float leapHeight = 0.0f,
            float followTargetMaxDistance = 0.0f,
            float backDistance = 0.0f,
            float travelTime = 0.0f
        ) : base(PacketCmd.PKT_S2C_Dash)
        {
            buffer.Write(Environment.TickCount); // syncID
            buffer.Write((short)1); // Number of dashes
            buffer.Write((byte)4); // Waypoints size * 2
            buffer.Write((uint)u.NetId);
            buffer.Write((float)dashSpeed);
            buffer.Write((float)leapHeight);
            buffer.Write((float)u.X);
            buffer.Write((float)u.Y);
            buffer.Write((byte)(keepFacingLastDirection ? 0x01 : 0x00));
            if (t.IsSimpleTarget)
            {
                buffer.Write((uint)0);
            }
            else
            {
                buffer.Write((uint)(t as GameObject).NetId);
            }

            buffer.Write((float)followTargetMaxDistance);
            buffer.Write((float)backDistance);
            buffer.Write((float)travelTime);

            var waypoints = new List<Vector2>
            {
                new Vector2(u.X, u.Y),
                new Vector2(t.X, t.Y)
            };

            buffer.Write(Movement.EncodeWaypoints(waypoints));
        }
    }
}
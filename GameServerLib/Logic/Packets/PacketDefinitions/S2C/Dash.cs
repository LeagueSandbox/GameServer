using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Dash : BasePacket
    {
        public Dash(Game game,
            AttackableUnit u,
            Target t,
            float dashSpeed,
            bool keepFacingLastDirection,
            float leapHeight = 0.0f,
            float followTargetMaxDistance = 0.0f,
            float backDistance = 0.0f,
            float travelTime = 0.0f
        ) : base(game, PacketCmd.PKT_S2C_DASH)
        {
            Write(Environment.TickCount); // syncID
            Write((short)1); // Number of dashes
            Write((byte)4); // Waypoints size * 2
            WriteNetId(u);
            Write(dashSpeed);
            Write(leapHeight);
            Write(u.X);
            Write(u.Y);
            Write((byte)(keepFacingLastDirection ? 0x01 : 0x00));
            if (t.IsSimpleTarget)
            {
                Write((uint)0);
            }
            else
            {
                WriteNetId(t as GameObject);
            }

            Write(followTargetMaxDistance);
            Write(backDistance);
            Write(travelTime);

            var waypoints = new List<Vector2>
            {
                new Vector2(u.X, u.Y),
                new Vector2(t.X, t.Y)
            };

            Write(Movement.EncodeWaypoints(game, waypoints));
        }
    }
}
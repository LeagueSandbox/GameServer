using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Dash : BasePacket
    {
        public Dash(AttackableUnit u,
            Target t,
            float dashSpeed,
            bool keepFacingLastDirection,
            float leapHeight = 0.0f,
            float followTargetMaxDistance = 0.0f,
            float backDistance = 0.0f,
            float travelTime = 0.0f
        ) : base(PacketCmd.PKT_S2_C_DASH)
        {
            _buffer.Write(Environment.TickCount); // syncID
            _buffer.Write((short)1); // Number of dashes
            _buffer.Write((byte)4); // Waypoints size * 2
            _buffer.Write((uint)u.NetId);
            _buffer.Write((float)dashSpeed);
            _buffer.Write((float)leapHeight);
            _buffer.Write((float)u.X);
            _buffer.Write((float)u.Y);
            _buffer.Write((byte)(keepFacingLastDirection ? 0x01 : 0x00));
            if (t.IsSimpleTarget)
            {
                _buffer.Write((uint)0);
            }
            else
            {
                _buffer.Write((uint)(t as GameObject).NetId);
            }

            _buffer.Write((float)followTargetMaxDistance);
            _buffer.Write((float)backDistance);
            _buffer.Write((float)travelTime);

            var waypoints = new List<Vector2>
            {
                new Vector2(u.X, u.Y),
                new Vector2(t.X, t.Y)
            };

            _buffer.Write(Movement.EncodeWaypoints(waypoints));
        }
    }
}
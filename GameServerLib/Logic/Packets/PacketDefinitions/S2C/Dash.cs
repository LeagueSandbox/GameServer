using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Dash : BasePacket
    {
        public Dash(DashArgs args) : base(PacketCmd.PKT_S2C_Dash)
        {
            buffer.Write(Environment.TickCount); // syncID
            buffer.Write((short)1); // Number of dashes
            buffer.Write((byte)4); // Waypoints size * 2
            buffer.Write((uint)args.Unit.UnitNetId);
            buffer.Write((float)args.DashSpeed);
            buffer.Write((float)args.LeapHeight);
            buffer.Write((float)args.Unit.X);
            buffer.Write((float)args.Unit.Y);
            buffer.Write((byte)(args.KeepFacingLastDirection ? 0x01 : 0x00));
            buffer.Write((uint)args.Target.UnitNetId);
            buffer.Write((float)args.FollowTargetMaxDistance);
            buffer.Write((float)args.BackDistance);
            buffer.Write((float)args.TravelTime);

            var waypoints = new List<Vector2>
            {
                new Vector2(args.Unit.X, args.Unit.Y),
                new Vector2(args.Target.X, args.Target.Y)
            };

            buffer.Write(Movement.EncodeWaypoints(waypoints));
        }
    }
}
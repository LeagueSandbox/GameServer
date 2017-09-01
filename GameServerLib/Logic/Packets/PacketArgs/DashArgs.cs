using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct DashArgs
    {
        public UnitAtLocation Unit { get; }
        public UnitAtLocation Target { get; }
        public float DashSpeed { get; }
        public bool KeepFacingLastDirection { get; }
        public float LeapHeight { get; }
        public float FollowTargetMaxDistance { get; }
        public float BackDistance { get; }
        public float TravelTime { get; }

        public DashArgs(UnitAtLocation unit, UnitAtLocation target, float dashSpeed, bool keepFacingLastDirection, float leapHeight = 0, float followTargetMaxDistance = 0, float backDistance = 0, float travelTime = 0)
        {
            Unit = unit;
            Target = target;
            DashSpeed = dashSpeed;
            KeepFacingLastDirection = keepFacingLastDirection;
            LeapHeight = leapHeight;
            FollowTargetMaxDistance = followTargetMaxDistance;
            BackDistance = backDistance;
            TravelTime = travelTime;
        }
    }
}

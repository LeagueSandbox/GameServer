using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class MovementAns : BasePacket
    {
        public MovementAns(GameObject obj) : this(new List<GameObject> { obj })
        {

        }

        public MovementAns(List<GameObject> actors) : base(PacketCmd.PKT_S2C_MoveAns)
        {
            buffer.Write(Environment.TickCount); // syncID
            buffer.Write((short)actors.Count);

            foreach (var actor in actors)
            {
                var waypoints = actor.Waypoints;
                var numCoords = waypoints.Count * 2;
                buffer.Write((byte)numCoords);
                buffer.Write((int)actor.NetId);
                buffer.Write(Movement.EncodeWaypoints(waypoints));
            }
        }

        private Pair<bool, bool> IsAbsolute(Vector2 vec)
        {
            var ret = new Pair<bool, bool>();
            ret.Item1 = vec.X < sbyte.MinValue || vec.X > sbyte.MaxValue;
            ret.Item2 = vec.Y < sbyte.MinValue || vec.Y > sbyte.MaxValue;

            return ret;
        }

        static void SetBitmaskValue(ref byte[] mask, int pos, bool val)
        {
            if (val)
                mask[pos / 8] |= (byte)(1 << (pos % 8));
            else
                mask[pos / 8] &= (byte)(~(1 << (pos % 8)));
        }
    }
}
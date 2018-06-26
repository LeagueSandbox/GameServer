using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MovementResponse : BasePacket
    {
        public MovementResponse(GameObject obj)
            : this(new List<GameObject> { obj })
        {

        }

        public MovementResponse(List<GameObject> actors)
            : base(PacketCmd.PKT_S2_C_MOVE_ANS)
        {
            _buffer.Write(Environment.TickCount); // syncID
            _buffer.Write((short)actors.Count);

            foreach (var actor in actors)
            {
                var waypoints = actor.Waypoints;
                var numCoords = waypoints.Count * 2;
                _buffer.Write((byte)numCoords);
                _buffer.Write((int)actor.NetId);
                _buffer.Write(Movement.EncodeWaypoints(waypoints));
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
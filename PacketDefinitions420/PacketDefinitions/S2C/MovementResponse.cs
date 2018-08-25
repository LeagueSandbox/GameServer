using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class MovementResponse : BasePacket
    {
        public MovementResponse(INavGrid navGrid, IGameObject obj)
            : this(navGrid, new List<IGameObject> { obj })
        {

        }

        public MovementResponse(INavGrid navGrid, List<IGameObject> actors)
            : base(PacketCmd.PKT_S2C_MOVE_ANS)
        {
            Write(Environment.TickCount); // syncID
            Write((short)actors.Count);

            foreach (var actor in actors)
            {
                var waypoints = actor.Waypoints;
                var numCoords = waypoints.Count * 2;
                Write((byte)numCoords);
                WriteNetId(actor);
                Write(Movement.EncodeWaypoints(navGrid, waypoints));
            }
        }

        private Pair<bool, bool> IsAbsolute(Vector2 vec)
        {
            var ret = new Pair<bool, bool>();
            ret.Item1 = vec.X < sbyte.MinValue || vec.X > sbyte.MaxValue;
            ret.Item2 = vec.Y < sbyte.MinValue || vec.Y > sbyte.MaxValue;

            return ret;
        }

        private static void SetBitmaskValue(ref byte[] mask, int pos, bool val)
        {
            if (val)
                mask[pos / 8] |= (byte)(1 << pos % 8);
            else
                mask[pos / 8] &= (byte)~(1 << pos % 8);
        }
    }
}
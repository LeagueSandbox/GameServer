using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MovementResponse : BasePacket
    {
        public MovementResponse(Game game, GameObject obj)
            : this(game, new List<GameObject> { obj })
        {

        }

        public MovementResponse(Game game, List<GameObject> actors)
            : base(game, PacketCmd.PKT_S2C_MOVE_ANS)
        {
            Write(Environment.TickCount); // syncID
            Write((short)actors.Count);

            foreach (var actor in actors)
            {
                var waypoints = actor.Waypoints;
                var numCoords = waypoints.Count * 2;
                Write((byte)numCoords);
                WriteNetId(actor);
                Write(Movement.EncodeWaypoints(game, waypoints));
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
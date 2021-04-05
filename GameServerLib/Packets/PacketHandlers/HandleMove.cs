using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleMove : PacketHandlerBase<MovementRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleMove(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, MovementRequest req)
        {
            var peerInfo = _playerManager.GetPeerInfo(userId);
            var champion = peerInfo?.Champion;
            if (peerInfo == null || !champion.CanMove())
            {
                return true;
            }

            // Last waypoint position
            var pos = req.Position;
            var translatedWaypoints = req.Waypoints.ConvertAll(TranslateFromCenteredCoordinates);

            var lastindex = 0;
            if (!(translatedWaypoints.Count - 1 < 0))
            {
                lastindex = translatedWaypoints.Count - 1;
            }

            var nav = _game.Map.NavigationGrid;

            foreach (Vector2 wp in translatedWaypoints)
            {
                if (!_game.Map.NavigationGrid.IsWalkable(wp))
                {
                    Vector2 exit = nav.GetClosestTerrainExit(translatedWaypoints[lastindex]);

                    // prevent player pathing within their collision radius
                    if (Vector2.DistanceSquared(champion.Position, exit) < (champion.CollisionRadius * champion.CollisionRadius))
                    {
                        return true;
                    }

                    if (_game.Map.NavigationGrid.IsWalkable(champion.Position))
                    {
                        translatedWaypoints = nav.GetPath(champion.Position, exit);
                    }
                    break;
                }
            }

            switch (req.Type)
            {
                case OrderType.AttackTo:
                    translatedWaypoints[0] = champion.Position;
                    champion.UpdateMoveOrder(OrderType.AttackTo, true);
                    // TODO: Verify if stopping movement is the correct thing to do.
                    champion.SetWaypoints(translatedWaypoints);
                    break;
                case OrderType.Stop:
                    champion.UpdateMoveOrder(OrderType.Stop, true);
                    break;
                case OrderType.Taunt:
                    champion.UpdateMoveOrder(OrderType.Taunt);
                    return true;
                case OrderType.AttackMove:
                    translatedWaypoints[0] = champion.Position;
                    champion.UpdateMoveOrder(OrderType.AttackMove, true);
                    champion.SetWaypoints(translatedWaypoints);
                    break;
                case OrderType.MoveTo:
                    translatedWaypoints[0] = champion.Position;
                    champion.UpdateMoveOrder(OrderType.MoveTo, true);
                    champion.SetWaypoints(translatedWaypoints);
                    break;
            }

            var u = _game.ObjectManager.GetObjectById(req.TargetNetId) as IAttackableUnit;
            champion.SetTargetUnit(u);

            if (translatedWaypoints == null)
            {
                return false;
            }

            return true;
        }

        private Vector2 TranslateFromCenteredCoordinates(Vector2 vector)
        {
            // For some reason, League coordinates are translated into center-based coordinates (origin at the center of the map),
            // so we have to translate them back into normal coordinates where the origin is at the bottom left of the map.
            return new Vector2(2 * vector.X + _game.Map.NavigationGrid.MiddleOfMap.X, 2 * vector.Y + _game.Map.NavigationGrid.MiddleOfMap.Y);
        }
    }
}

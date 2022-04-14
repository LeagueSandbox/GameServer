﻿using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Handlers;
using System.Numerics;
using System.Collections.Generic;

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
            if (peerInfo == null)
            {
                return true;
            }

            if (champion.MovementParameters == null)
            {
                // Last waypoint position
                List<Vector2> translatedWaypoints = req.Waypoints.ConvertAll(TranslateFromCenteredCoordinates);
                var lastindex = 0;
                if (!(translatedWaypoints.Count - 1 < 0))
                {
                    lastindex = translatedWaypoints.Count - 1;
                }

                var nav = _game.Map.NavigationGrid;

                foreach (Vector2 wp in translatedWaypoints)
                {
                    if (!_game.Map.PathingHandler.IsWalkable(wp))
                    {
                        Vector2 exit = nav.GetClosestTerrainExit(translatedWaypoints[lastindex]);

                        // prevent player pathing within their pathing radius
                        if (Vector2.DistanceSquared(champion.Position, exit) < (champion.PathfindingRadius * champion.PathfindingRadius))
                        {
                            return true;
                        }

                        if (_game.Map.PathingHandler.IsWalkable(champion.Position))
                        {
                            translatedWaypoints = _game.Map.PathingHandler.GetPath(champion.Position, exit);
                        }
                        break;
                    }
                }

                switch ((OrderType)req.OrderType)
                {
                    case OrderType.AttackTo:
                        translatedWaypoints[0] = champion.Position;
                        champion.UpdateMoveOrder(OrderType.AttackTo, true);
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

                if (translatedWaypoints == null)
                {
                    return false;
                }
            }

            var u = _game.ObjectManager.GetObjectById(req.TargetNetID) as IAttackableUnit;
            champion.SetTargetUnit(u);

            if (champion.SpellToCast != null)
            {
                champion.SetSpellToCast(null, Vector2.Zero);
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

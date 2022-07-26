using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Enums;
using GameServerCore.Packets.Handlers;
using System.Numerics;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Players;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleMove : PacketHandlerBase<MovementRequest>
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public HandleMove(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, MovementRequest req)
        {
            var peerInfo = _playerManager.GetPeerInfo(userId);
            if (peerInfo == null)
            {
                return true;
            }

            var champion = peerInfo.Champion;
            if (champion.MovementParameters == null)
            {
                var nav = _game.Map.NavigationGrid;

                Vector2 lastWaypoint = TranslateFromCenteredCoordinates(req.Waypoints[req.Waypoints.Count - 1]);
                List<Vector2> translatedWaypoints = nav.GetPath(champion.Position, lastWaypoint, champion.PathfindingRadius);
                if (translatedWaypoints == null)
                {
                    nav.GetPath(champion.Position, lastWaypoint, champion.PathfindingRadius, true);
                    return false;
                }

                var u = _game.ObjectManager.GetObjectById(req.TargetNetID) as AttackableUnit;
                var pet = champion.GetPet();

                switch (req.OrderType)
                {
                    case OrderType.MoveTo:
                        translatedWaypoints[0] = champion.Position;
                        champion.UpdateMoveOrder(OrderType.MoveTo, true);
                        champion.SetWaypoints(translatedWaypoints);
                        champion.SetTargetUnit(u);
                        break;
                    case OrderType.AttackTo:
                        translatedWaypoints[0] = champion.Position;
                        champion.UpdateMoveOrder(OrderType.AttackTo, true);
                        champion.SetWaypoints(translatedWaypoints);
                        champion.SetTargetUnit(u);
                        break;
                    case OrderType.PetHardAttack:
                        if (pet != null)
                        {
                            List<Vector2> waypoints = _game.Map.PathingHandler.GetPath(pet.Position, nav.GetClosestTerrainExit(req.Position), pet.PathfindingRadius);
                            pet.UpdateMoveOrder(OrderType.PetHardAttack, true);
                            pet.SetWaypoints(waypoints);
                            pet.SetTargetUnit(u, true);
                        }
                        break;
                    case OrderType.PetHardMove:
                        if (pet != null)
                        {
                            List<Vector2> waypoints = _game.Map.PathingHandler.GetPath(pet.Position, nav.GetClosestTerrainExit(req.Position), pet.PathfindingRadius);
                            pet.UpdateMoveOrder(OrderType.PetHardMove, true);
                            pet.SetWaypoints(waypoints);
                            pet.SetTargetUnit(u, true);
                        }
                        break;
                    case OrderType.AttackMove:
                        translatedWaypoints[0] = champion.Position;
                        champion.UpdateMoveOrder(OrderType.AttackMove, true);
                        champion.SetWaypoints(translatedWaypoints);
                        champion.SetTargetUnit(u);
                        break;
                    case OrderType.Taunt:
                        champion.UpdateMoveOrder(OrderType.Taunt);
                        return true;
                    case OrderType.PetHardReturn:
                        if (pet != null)
                        {
                            List<Vector2> waypoints = _game.Map.PathingHandler.GetPath(pet.Position, nav.GetClosestTerrainExit(req.Position), pet.PathfindingRadius);
                            pet.UpdateMoveOrder(OrderType.PetHardReturn, true);
                            pet.SetWaypoints(waypoints);
                            pet.SetTargetUnit(u, true);
                        }
                        break;
                    case OrderType.Stop:
                        champion.UpdateMoveOrder(OrderType.Stop, true);
                        break;
                    case OrderType.PetHardStop:
                        if (pet != null)
                        {
                            pet.UpdateMoveOrder(OrderType.PetHardStop, true);
                        }
                        break;
                    case OrderType.Use:
                        translatedWaypoints[0] = champion.Position;
                        champion.UpdateMoveOrder(OrderType.Use, true);
                        champion.SetWaypoints(translatedWaypoints);
                        champion.SetTargetUnit(u);
                        break;
                }
            }

            // TODO: Shouldn't be here.
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

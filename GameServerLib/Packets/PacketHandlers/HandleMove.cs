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

                var u = _game.ObjectManager.GetObjectById(req.TargetNetID) as AttackableUnit;
                var pet = champion.GetPet();
                List<Vector2> waypoints;

                switch (req.OrderType)
                {
                    case OrderType.MoveTo:
                    case OrderType.AttackTo:
                    case OrderType.AttackMove:
                    case OrderType.Use:
                        waypoints = nav.GetPath(champion.Position, req.Position, champion.PathfindingRadius);
                        if (waypoints == null)
                        {
                            return false;
                        }
                        champion.UpdateMoveOrder(req.OrderType, true);
                        champion.SetWaypoints(waypoints);
                        champion.SetTargetUnit(u);
                        break;
                    case OrderType.PetHardAttack:
                    case OrderType.PetHardMove:
                    case OrderType.PetHardReturn:
                        if (pet != null)
                        {
                            waypoints = nav.GetPath(pet.Position, req.Position, pet.PathfindingRadius);
                            if (waypoints == null)
                            {
                                return false;
                            }
                            pet.UpdateMoveOrder(req.OrderType, true);
                            pet.SetWaypoints(waypoints);
                            pet.SetTargetUnit(u, true);
                        }
                        break;
                    case OrderType.Taunt:
                        champion.UpdateMoveOrder(req.OrderType);
                        return true;
                    case OrderType.Stop:
                        champion.UpdateMoveOrder(req.OrderType, true);
                        break;
                    case OrderType.PetHardStop:
                        if (pet != null)
                        {
                            pet.UpdateMoveOrder(req.OrderType, true);
                        }
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

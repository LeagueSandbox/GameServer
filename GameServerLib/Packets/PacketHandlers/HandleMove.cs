using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using System;
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
            var peerInfo = _playerManager.GetPeerInfo((ulong)userId);
            var champion = peerInfo?.Champion;
            if (peerInfo == null || !champion.CanMove())
            {
                return true;
            }
            // Last waypoint position
            var pos = req.Position;
            var translatedWaypoints = req.Waypoints.ConvertAll(TranslateFromCenteredCoordinates);
            translatedWaypoints.Insert(0, champion.GetPosition());
            switch (req.Type)
            {
                case MoveType.STOP:
                    champion.UpdateMoveOrder(MoveOrder.MOVE_ORDER_MOVE);
                    champion.StopMovement();
                    break;
                case MoveType.EMOTE:
                    //Logging->writeLine("Emotion");
                    return true;
                case MoveType.ATTACKMOVE:

                    champion.UpdateMoveOrder(MoveOrder.MOVE_ORDER_ATTACKMOVE);
                    champion.SetWaypoints(translatedWaypoints);
                    break;
                case MoveType.MOVE:
                    champion.UpdateMoveOrder(MoveOrder.MOVE_ORDER_MOVE);
                    champion.SetWaypoints(translatedWaypoints);
                    break;
            }

            var u = _game.ObjectManager.GetObjectById(req.TargetNetId) as IAttackableUnit;
            champion.UpdateTargetUnit(u);
            return true;
        }
        private Vector2 TranslateFromCenteredCoordinates(Vector2 vector)
        {
            // For ???? reason coordinates are translated to 0,0 as a map center, so we gotta get back the original
            // mapSize contains the real center point coordinates, meaning width/2, height/2
            return new Vector2(2 * vector.X + _game.Map.NavGrid.GetSize().X, 2 * vector.Y + _game.Map.NavGrid.GetSize().Y);
        }
    }
}

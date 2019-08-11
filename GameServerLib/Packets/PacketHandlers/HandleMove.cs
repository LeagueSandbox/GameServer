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
            var peerInfo = _playerManager.GetPeerInfo((ulong)userId);
            var champion = peerInfo?.Champion;
            if (peerInfo == null || !champion.CanMove())
            {
                return true;
            }
            // Last waypoint position
            var pos = req.Position;
            var translatedWaypoints = req.Waypoints.ConvertAll(TranslateFromCenteredCoordinates);
            var nav = _game.Map.NavGrid;
            translatedWaypoints = nav.GetPath(champion.GetPosition(), nav.GetClosestTerrainExit(req.Position), champion.CollisionRadius);
            if (translatedWaypoints == null) return false;
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
                    translatedWaypoints[0] = champion.GetPosition();
                    champion.UpdateMoveOrder(MoveOrder.MOVE_ORDER_ATTACKMOVE);
                    champion.SetWaypoints(translatedWaypoints);
                    break;
                case MoveType.MOVE:
                    translatedWaypoints[0] = champion.GetPosition();
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

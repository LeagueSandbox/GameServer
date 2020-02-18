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
            var vMoves = ReadWaypoints(req.MoveData, req.CoordCount, _game.Map);

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
                    vMoves[0] = new Vector2(champion.X,champion.Y);
                    champion.UpdateMoveOrder(MoveOrder.MOVE_ORDER_ATTACKMOVE);
                    champion.SetWaypoints(vMoves);
                    break;
                case MoveType.MOVE:
                    vMoves[0] = new Vector2(champion.X, champion.Y);
                    champion.UpdateMoveOrder(MoveOrder.MOVE_ORDER_MOVE);
                    champion.SetWaypoints(vMoves);
                    break;
            }

            var u = _game.ObjectManager.GetObjectById(req.TargetNetId) as IAttackableUnit;
            champion.UpdateTargetUnit(u);
            return true;
        }

        private List<Vector2> ReadWaypoints(byte[] buffer, int coordCount, IMap map)
        {
            if (coordCount % 2 > 0)
            {
                coordCount++;
            }

            List<Vector2> vMoves = new List<Vector2>();
            using (BinaryReader reader = new BinaryReader(new MemoryStream(buffer)))
            {
                BitArray mask = null;
                if (coordCount > 2)
                {
                    mask = new BitArray(reader.ReadBytes((coordCount - 3) / 8 + 1));
                }

                Vector2 lastCoord = new Vector2(reader.ReadInt16(), reader.ReadInt16());
                vMoves.Add(TranslateCoordinates(lastCoord, map.NavigationGrid.MiddleOfMap));

                if (coordCount < 3)
                {
                    return vMoves;
                }

                for (int i = 0; i < coordCount - 2; i += 2)
                {
                    if (mask[i])
                    {
                        lastCoord.X += reader.ReadSByte();
                    }
                    else
                    {
                        lastCoord.X = reader.ReadInt16();
                    }

                    if (mask[i + 1])
                    {
                        lastCoord.Y += reader.ReadSByte();
                    }
                    else
                    {
                        lastCoord.Y = reader.ReadInt16();
                    }

                    vMoves.Add(TranslateCoordinates(lastCoord, map.NavigationGrid.MiddleOfMap));
                }
            }

            return vMoves;
        }

        private Vector2 TranslateCoordinates(Vector2 vector, Vector2 mapCenter)
        {
            // For ???? reason coordinates are translated to 0,0 as a map center, so we gotta get back the original
            // mapCenter contains the real center point coordinates, meaning width/2, height/2
            return new Vector2(2 * vector.X + mapCenter.X, 2 * vector.Y + mapCenter.Y);
        }
    }
}

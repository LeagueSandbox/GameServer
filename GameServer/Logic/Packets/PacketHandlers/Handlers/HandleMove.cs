using System.Collections.Generic;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Maps;
using System.IO;
using System.Collections;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleMove : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var peerInfo = _playerManager.GetPeerInfo(peer);
            var champion = peerInfo.Champion;
            if (peerInfo == null ||
                champion.IsDashing ||
                champion.IsDead ||
                champion.IsCastingSpell)
                return true;

            var request = new MovementReq(data);
            var vMoves = readWaypoints(request.moveData, request.coordCount, _game.Map);

            switch (request.type)
            {
                case MoveType.STOP:
                    //TODO anticheat, currently it trusts client 100%

                    peerInfo.Champion.setPosition(request.x, request.y);
                    float x = ((request.x) - _game.Map.GetWidth()) / 2;
                    float y = ((request.y) - _game.Map.GetHeight()) / 2;

                    for (var i = 0; i < vMoves.Count; i++)
                    {
                        var v = vMoves[i];
                        v.X = (short)request.x;
                        v.Y = (short)request.y;
                    }
                    break;
                case MoveType.EMOTE:
                    //Logging->writeLine("Emotion");
                    return true;
                case MoveType.ATTACKMOVE:
                    peerInfo.Champion.MoveOrder = MoveOrder.MOVE_ORDER_ATTACKMOVE;
                    break;
                case MoveType.MOVE:
                    peerInfo.Champion.MoveOrder = MoveOrder.MOVE_ORDER_MOVE;
                    break;
            }

            vMoves[0] = new Vector2(peerInfo.Champion.X, peerInfo.Champion.Y);
            peerInfo.Champion.SetWaypoints(vMoves);

            var u = _game.Map.GetObjectById(request.targetNetId) as Unit;
            if (u == null)
            {
                peerInfo.Champion.TargetUnit = null;
                return true;
            }

            peerInfo.Champion.TargetUnit = u;

            return true;
        }

        private List<Vector2> readWaypoints(byte[] buffer, int coordCount, Map map)
        {
            if (coordCount % 2 > 0)
                coordCount++;

            var mapSize = map.GetSize();
            var reader = new BinaryReader(new MemoryStream(buffer));

            BitArray mask = null;
            if (coordCount > 2)
                mask = new BitArray(reader.ReadBytes(((coordCount - 3) / 8) + 1));
            var lastCoord = new Vector2(reader.ReadInt16(), reader.ReadInt16());
            var vMoves = new List<Vector2> { TranslateCoordinates(lastCoord, mapSize) };

            if (coordCount < 3)
                return vMoves;

            for (int i = 0; i < coordCount - 2; i += 2)
            {
                if (mask[i])
                    lastCoord.X += reader.ReadSByte();
                else
                    lastCoord.X = reader.ReadInt16();

                if (mask[i + 1])
                    lastCoord.Y += reader.ReadSByte();
                else
                    lastCoord.Y = reader.ReadInt16();
                vMoves.Add(TranslateCoordinates(lastCoord, mapSize));
            }
            return vMoves;
        }

        private Vector2 TranslateCoordinates(Vector2 vector, Vector2 mapSize)
        {
            // For ???? reason coordinates are translated to 0,0 as a map center, so we gotta get back the original
            // mapSize contains the real center point coordinates, meaning width/2, height/2
            return new Vector2(2 * vector.X + mapSize.X, 2 * vector.Y + mapSize.Y);
        }
    }
}

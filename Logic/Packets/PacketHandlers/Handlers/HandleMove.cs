using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Maps;
using System.IO;
using System.Collections;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleMove : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var peerInfo = game.GetPeerInfo(peer);
            if (peerInfo == null || peerInfo.GetChampion().isDashing() || peerInfo.GetChampion().isDead())
                return true;

            var request = new MovementReq(data);
            var vMoves = readWaypoints(request.moveData, request.coordCount, game.GetMap());

            switch (request.type)
            {
                case MoveType.STOP:
                    //TODO anticheat, currently it trusts client 100%

                    peerInfo.GetChampion().setPosition(request.x, request.y);
                    float x = ((request.x) - game.GetMap().GetWidth()) / 2;
                    float y = ((request.y) - game.GetMap().GetHeight()) / 2;

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
                    peerInfo.GetChampion().setMoveOrder(MoveOrder.MOVE_ORDER_ATTACKMOVE);
                    break;
                case MoveType.MOVE:
                    peerInfo.GetChampion().setMoveOrder(MoveOrder.MOVE_ORDER_MOVE);
                    break;
            }

            vMoves[0] = new Vector2(peerInfo.GetChampion().getX(), peerInfo.GetChampion().getY());
            peerInfo.GetChampion().setWaypoints(vMoves);

            var u = game.GetMap().GetObjectById(request.targetNetId) as Unit;
            if (u == null)
            {
                peerInfo.GetChampion().setTargetUnit(null);
                return true;
            }

            peerInfo.GetChampion().setTargetUnit(u);

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

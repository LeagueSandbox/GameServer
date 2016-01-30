using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;
using IntWarsSharp.Logic.Packets;
using System.Numerics;
using IntWarsSharp.Logic.GameObjects;

namespace IntWarsSharp.Core.Logic.PacketHandlers.Packets
{
    class HandleMove : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            var peerInfo = game.peerInfo(peer);
            if (peerInfo == null || peerInfo.getChampion().isDashing() || peerInfo.getChampion().isDead())
                return true;

            var request = new MovementReq(data, game.getMap());
            var vMoves = readWaypoints(request.moveData, request.coordCount);

            switch (request.type)
            {
                case MoveType.STOP:
                    {
                        //TODO anticheat, currently it trusts client 100%

                        peerInfo.getChampion().setPosition(request.x, request.y);
                        float x = ((request.x) - game.getMap().getWidth()) / 2;
                        float y = ((request.y) - game.getMap().getHeight()) / 2;

                        for (var i = 0; i < vMoves.Count; i++)
                        {
                            var v = vMoves[i];
                            v.X = (short)request.x;
                            v.Y = (short)request.y;
                        }

                        Logger.LogCoreInfo("Stopped at x: " + request.x + ", y: " + request.y);
                        break;
                    }
                case MoveType.EMOTE:
                    //Logging->writeLine("Emotion");
                    return true;
                case MoveType.ATTACKMOVE:
                    peerInfo.getChampion().setMoveOrder(MoveOrder.MOVE_ORDER_ATTACKMOVE);
                    break;
                case MoveType.MOVE:
                    peerInfo.getChampion().setMoveOrder(MoveOrder.MOVE_ORDER_MOVE);
                    break;
            }

            // Sometimes the client will send a wrong position as the first one, override it with server data
            vMoves[0] = new Vector2(peerInfo.getChampion().getX(), peerInfo.getChampion().getY());
            peerInfo.getChampion().setWaypoints(vMoves);
            var u = game.getMap().getObjectById(request.targetNetId) as Unit;
            if (u == null)
            {
                peerInfo.getChampion().setTargetUnit(null);
                return true;
            }

            peerInfo.getChampion().setTargetUnit(u);

            return true;
        }

        private List<Vector2> readWaypoints(byte[] buffer, int coordCount)
        {
            var nPos = (coordCount + 5) / 8;
            if (coordCount % 2 > 0)
                nPos++;

            var vectorCount = coordCount / 2;
            var vMoves = new List<Vector2>();
            var lastCoord = new Vector2();
            for (int i = 0; i < vectorCount; i++)
            {
                if (GetBitmaskValue(buffer, (i - 1) * 2))
                {
                    lastCoord.X += buffer[nPos++];
                }
                else
                {
                    lastCoord.X = buffer[nPos];
                    nPos += 2;
                }
                if (GetBitmaskValue(buffer, (i - 1) * 2 + 1))
                {
                    lastCoord.Y += buffer[nPos++];
                }
                else
                {
                    lastCoord.Y = buffer[nPos];
                    nPos += 2;
                }
                vMoves.Add(lastCoord);
            }
            return vMoves;
        }
        private bool GetBitmaskValue(byte[] mask, int pos)
        {
            return pos >= 0 && ((1 << (pos % 8)) & mask[pos / 8]) != 0;
        }
    }
}

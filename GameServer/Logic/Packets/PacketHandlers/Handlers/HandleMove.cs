﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;
using LeagueSandbox.GameServer.Logic.Packets;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Maps;
using System.IO;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleMove : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            var peerInfo = game.getPeerInfo(peer);
            if (peerInfo == null || peerInfo.getChampion().isDashing() || peerInfo.getChampion().isDead())
                return true;

            var request = new MovementReq(data);
            var vMoves = new List<Vector2>();//readWaypoints(request.moveData, request.coordCount, game.getMap());
            vMoves.Add(new Vector2(peerInfo.getChampion().getX(), peerInfo.getChampion().getY()));
            vMoves.Add(new Vector2(request.x, request.y)); // TODO
            switch (request.type)
            {
                case MoveType.STOP:
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
                    break;
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

        private List<Vector2> readWaypoints(byte[] buffer, int coordCount, Map map)
        {
            var reader = new BinaryReader(new MemoryStream(buffer));
            var mapSize = map.getSize();
            int vectorCount = coordCount / 2;
            var vMoves = new List<Vector2>();
            var lastCoord = new Vector2(0.0f, 0.0f);

            reader.BaseStream.Position = (coordCount + 5) / 8 + coordCount % 2;

            for (int i = 0; i < vectorCount; i++)
            {
                if (GetBitmaskValue(buffer, 2 * i - 2))
                    lastCoord.X += reader.ReadByte();
                else
                    lastCoord.X = reader.ReadInt16();
                if (GetBitmaskValue(buffer, 2 * i - 1))
                    lastCoord.Y += reader.ReadByte();
                else
                    lastCoord.Y = reader.ReadInt16();
                vMoves.Add(new Vector2(2.0f * lastCoord.X + mapSize.X, 2.0f * lastCoord.Y + mapSize.Y));
            }
            return vMoves;
        }
        private bool GetBitmaskValue(byte[] mask, int pos)
        {
            return pos >= 0 && ((1 << (pos % 8)) & mask[pos / 8]) != 0;
        }
    }
}

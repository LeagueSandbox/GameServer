using GameServerCore.Domain.GameObjects;
using GameServerCore.NetInfo;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    class GameEndResponse : ICoreResponse
    {
        public Vector3 CameraPosition { get; }
        public INexus Nexus { get; }
        public List<Tuple<uint, ClientInfo>>  Players {get;}
        public GameEndResponse(Vector3 cameraPosition, INexus nexus, List<Tuple<uint, ClientInfo>> playersUserId)
        {
            CameraPosition = cameraPosition;
            Nexus = nexus;
            Players = playersUserId;
        }
    }
}

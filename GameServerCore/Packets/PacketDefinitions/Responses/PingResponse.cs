using GameServerCore.Domain.GameObjects;
using GameServerCore.NetInfo;
using GameServerCore.Packets.Enums;
using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    class PingResponse : ICoreResponse
    {
        public ClientInfo Client { get; }
        public Vector2 Position { get; }
        public IAttackableUnit Target {get;}
        public Pings PingType{ get; }
        public PingResponse(ClientInfo client, Vector2 pos, IAttackableUnit target, Pings type)
        {
            Client = client;
            Position = pos;
            Target = target;
            PingType = type;
        }
    }
}

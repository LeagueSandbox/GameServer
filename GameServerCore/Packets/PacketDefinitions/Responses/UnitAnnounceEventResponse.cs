using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Collections.Generic;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class UnitAnnounceEventResponse : ICoreResponse
    {
        public UnitAnnounces MessageId { get; }
        public IAttackableUnit Target { get; }
        public IGameObject Killer { get; }
        public List<IChampion> Assists { get; }
        public UnitAnnounceEventResponse(UnitAnnounces messageId, IAttackableUnit target, IGameObject killer = null, List<IChampion> assists = null)
        {
            MessageId = messageId;
            Target = target;
            Killer = killer;
            Assists = assists;
        }
    }
};
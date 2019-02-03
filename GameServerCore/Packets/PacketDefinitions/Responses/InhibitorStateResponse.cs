using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class InhibitorStateResponse : ICoreResponse
    {
        public IInhibitor Inhibitor { get; }
        public IGameObject Killer { get; }
        public List<IChampion> Assists { get; }
        public InhibitorStateResponse(IInhibitor inhibitor, IGameObject killer = null, List<IChampion> assists = null)
        {
            Inhibitor = inhibitor;
            Killer = killer;
            Assists = assists;
        }
    }
}
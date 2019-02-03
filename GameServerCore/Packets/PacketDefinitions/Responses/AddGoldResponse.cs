using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class AddGoldResponse : ICoreResponse
    {
        public IChampion Champ { get; }
        public IAttackableUnit Died { get; }
        public float Gold { get; }
        public AddGoldResponse(IChampion c, IAttackableUnit died, float gold)
        {
            Champ = c;
            Died = died;
            Gold = gold;
        }
    }
}
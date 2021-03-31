using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class EditBuffResponse : ICoreResponse
    {
        public IBuff BuffObj { get; }
        public int Stacks { get; }
        public EditBuffResponse(IBuff b, int stacks)
        {
            BuffObj = b;
            Stacks = stacks;
        }
    }
}
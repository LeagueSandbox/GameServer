using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ResumeGameResponse : ICoreResponse
    {
        public IAttackableUnit Unpauser { get; }
        public bool ShowWindow { get; }
        public ResumeGameResponse(IAttackableUnit unpauser, bool showWindow)
        {
            Unpauser = unpauser;
            ShowWindow = showWindow;
        }
    }
}
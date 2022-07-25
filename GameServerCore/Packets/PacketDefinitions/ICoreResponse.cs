namespace GameServerCore.Packets.PacketDefinitions
{
    /// <summary>
    /// Interface for packet responses. *NOTE*: Currently empty as responses are handled independently.
    /// Refer to GameServerCore.Packets.PacketDefinitions.Responses for response definitions. Unused compared to ICoreRequest.
    /// </summary>
    public interface ICoreResponse : ICoreMessage
    {
    }
}

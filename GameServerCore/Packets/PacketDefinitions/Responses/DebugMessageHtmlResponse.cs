namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class DebugMessageHtmlResponse : ICoreResponse
    {
        public string HtmlDebugMessage { get; }
        public DebugMessageHtmlResponse(string htmlDebugMessage)
        {
            HtmlDebugMessage = htmlDebugMessage;
        }
    }
};
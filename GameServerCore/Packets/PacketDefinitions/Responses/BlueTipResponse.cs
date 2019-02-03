namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class BlueTipResponse : ICoreResponse
    {
        public int UserId { get; }
        public string Title { get; }
        public string Text { get; }
        public string ImagePath { get; }
        public byte TipCommand { get; }
        public uint PlayerNetId { get; }
        public uint TargetNetId { get; }
        public BlueTipResponse(int userId, string title, string text, string imagePath, byte tipCommand, uint playerNetId, uint targetNetId)
        {
            UserId = userId;
            Title = title;
            Text = text;
            ImagePath = imagePath;
            TipCommand = tipCommand;
            PlayerNetId = playerNetId;
            TargetNetId = targetNetId;
        }
    }
}
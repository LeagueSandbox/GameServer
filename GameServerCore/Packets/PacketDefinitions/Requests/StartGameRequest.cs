namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class StartGameRequest : ICoreRequest
    {
        //TODO: Change these for enums
        public sbyte TipID { get; }
        public sbyte ColorID { get; }
        public sbyte DurationID { get; }
        public sbyte Flags { get; }

        public StartGameRequest(sbyte tipId, sbyte colorId, sbyte durationId, sbyte flags)
        {
            TipID = tipId;
            ColorID = colorId;
            DurationID = durationId;
            Flags = flags;
        }
    }
}

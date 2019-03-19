namespace GameServerCore.Domain
{
    public interface IAnnounce
    {
        bool IsAnnounced { get; }
        long EventTime { get; }
        void Execute();
    }
}
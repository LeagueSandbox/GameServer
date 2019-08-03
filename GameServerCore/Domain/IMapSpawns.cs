namespace LeagueSandbox.GameServer
{
    public interface IMapSpawns
    {
        void SetSpawns(string team, IPlayerSpawns spawns, int playerCount);
    }
}
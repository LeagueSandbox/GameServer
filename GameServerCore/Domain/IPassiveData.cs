namespace LeagueSandbox.GameServer.Content
{
    public interface IPassiveData
    {
        string PassiveNameStr { get; set; }
        string PassiveLuaName { get; set; }
        int[] PassiveLevels { get; set; }
    }
}
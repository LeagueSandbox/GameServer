namespace LeagueSandbox.GameServer.Content
{
    public interface IPassiveData
    {
        string PassiveNameStr { get; set; }
        string PassiveAbilityName { get; set; }
        int[] PassiveLevels { get; set; }
    }
}
namespace GameServerCore.Scripting.CSharp
{
    public interface ITalent: IEventSource
    {
        string Name { get; }
        byte Rank { get; }
        ITalentScript Script { get; }
    }
}

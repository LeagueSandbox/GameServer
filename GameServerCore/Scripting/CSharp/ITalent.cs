namespace GameServerCore.Scripting.CSharp
{
    public interface ITalent
    {
        string Name { get; }
        byte Rank { get; }
        ITalentScript Script { get; }
    }
}

namespace GameServerCore.Domain.GameObjects
{
    public interface IStatModifier
    {
        float BaseBonus { get; set; }
        float PercentBaseBonus { get; set; }
        float FlatBonus { get; set; }
        float PercentBonus { get; set; }
        bool StatModified { get; }
    }
}

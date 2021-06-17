namespace GameServerCore.Domain.GameObjects
{
    public interface IStatModifier
    {
        float BaseValue { get; set; }
        float BaseBonus { get; set; }
        float PercentBaseBonus { get; set; }
        float FlatBonus { get; set; }
        float PercentBonus { get; set; }
        bool StatModified { get; }
    }
}

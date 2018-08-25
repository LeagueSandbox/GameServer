namespace GameServerCore.Domain.GameObjects
{
    public interface IStat
    {
        bool Modified { get; }
        float BaseBonus { get; }
        float FlatBonus { get; set; }
        float BaseValue { get; set; }
        float PercentBonus { get; }
        float PercentBaseBonus { get; }
        float Total { get; }
    }
}

namespace GameServerCore.Domain.GameObjects
{
    public interface IStat
    {
        bool Modified { get; }
        float BaseBonus { get; }
        float FlatBonus { get; set; }
        float BaseValue { get; set; }
        float PercentBonus { get; set;  }
        float PercentBaseBonus { get; }
        float Total { get; }
        bool ApplyStatModificator(IStatModifier statModifcator);
        bool RemoveStatModificator(IStatModifier statModifcator);
    }
}

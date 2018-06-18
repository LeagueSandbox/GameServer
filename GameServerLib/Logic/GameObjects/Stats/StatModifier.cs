namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class StatModifier
    {
        public float BaseBonus { get; set; }
        public float PercentBaseBonus { get; set; }
        public float FlatBonus { get; set; }
        public float PercentBonus { get; set; }
        public bool StatModified => (
            BaseBonus != 0 ||
            PercentBaseBonus != 0 ||
            FlatBonus != 0 ||
            PercentBonus != 0
        );
    }
}

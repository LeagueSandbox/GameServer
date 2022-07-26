namespace LeagueSandbox.GameServer.Content
{
    public class DamageRatios
    {
        public float HeroToHero { get; set; } = 1.0f;
        public float BuildingToHero { get; set; } = 1.0f;
        public float UnitToHero { get; set; } = 0.6f;
        public float HeroToUnit { get; set; } = 1.0f;
        public float BuildingToUnit { get; set; } = 1.25f;
        public float UnitToUnit { get; set; } = 1.0f;
        public float HeroToBuilding { get; set; } = 1.0f;
        public float BuildingToBuilding { get; set; } = 1.0f;
        public float UnitToBuilding { get; set; } = 0.5f;
    }
}

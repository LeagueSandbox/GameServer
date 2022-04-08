using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using System.Numerics;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public class MapScriptMetadata : IMapScriptMetadata
    {
        public float BaseGoldPerGoldTick { get; set; } = 0.95f;
        public float ChampionBaseGoldValue { get; set; } = 300.0f;
        public float ChampionMaxGoldValue { get; set; } = 500.0f;
        public float ChampionMinGoldValue { get; set; } = 50.0f;
        public string ExpCurveOverride { get; set; } = string.Empty;
        public float ExpRange { get; set; } = 1400.0f;
        public float FirstBloodExtraGold { get; set; } = 100.0f;
        public float FirstGoldTime { get; set; } = 90 * 1000;
        public float GoldTickSpeed { get; set; } = 500f;
        public float GoldRange { get; set; } = 1250.0f;
        public int InitialLevel { get; set; } = 1;
        public bool IsKillGoldRewardReductionActive { get; set; } = true;
        public int MaxLevel { get; set; } = 18;
        public bool MinionSpawnEnabled { get; set; } = false;
        public string NavGridOverride { get; set; } = string.Empty;
        public bool OverrideSpawnPoints { get; set; } = false;
        public int RecallSpellItemId { get; set; } = 2001;
        public long SpawnInterval { get; set; } = 30 * 1000;
        public float StartingGold { get; set; } = 475.0f;
    }
}

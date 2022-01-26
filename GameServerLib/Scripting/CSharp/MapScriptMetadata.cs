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
        public bool EnableBuildingProtection { get; set; } = false;

        //Stuff about minions
        public bool MinionSpawnEnabled { get; set; } = false;
        public long SpawnInterval { get; set; } = 30 * 1000;
        public bool MinionPathingOverride { get; set; } = false;

        //General things that will affect players globaly, such as default gold per-second, Starting gold....
        public float GoldPerSecond { get; set; } = 1.9f;
        public float StartingGold { get; set; } = 475.0f;
        public bool IsKillGoldRewardReductionActive { get; set; } = true;
        public int RecallSpellItemId { get; set; } = 2001;
        public long FirstGoldTime { get; set; } = 90 * 1000;
        public bool EnableFountainHealing { get; set; } = true;
        public bool OverrideSpawnPoints { get; set; } = false;
    }
}

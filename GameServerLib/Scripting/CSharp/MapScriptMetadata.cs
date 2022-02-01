﻿using GameServerCore.Domain.GameObjects;
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
        public bool EnableFountainHealing { get; set; } = true;
        public float FirstGoldTime { get; set; } = 90 * 1000;
        public float GoldPerSecond { get; set; } = 1.9f;
        public int InitialLevel { get; set; } = 1;
        public bool IsKillGoldRewardReductionActive { get; set; } = true;
        public int MaxLevel { get; set; } = 18;
        public bool MinionPathingOverride { get; set; } = false;
        public bool MinionSpawnEnabled { get; set; } = false;
        public bool OverrideSpawnPoints { get; set; } = false;
        public int RecallSpellItemId { get; set; } = 2001;
        public long SpawnInterval { get; set; } = 30 * 1000;
        public float StartingGold { get; set; } = 475.0f;
    }
}

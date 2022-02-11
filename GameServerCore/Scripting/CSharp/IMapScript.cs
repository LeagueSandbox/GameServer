using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using GameServerCore.Scripting.CSharp;

namespace GameServerCore.Domain
{
    public interface IMapScript : IUpdate
    {
        IGlobalData GlobalData { get; }
        IMapScriptMetadata MapScriptMetadata { get; }
        bool HasFirstBloodHappened { get; set; }
        long NextSpawnTime { get; set; }
        string LaneMinionAI { get; }
        string LaneTurretAI { get; }
        Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; }
        Dictionary<TurretType, int[]> TurretItems { get; }
        Dictionary<TeamId, string> NexusModels { get; }
        Dictionary<TeamId, string> InhibitorModels { get; }
        Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; }
        Dictionary<TeamId, Dictionary<MinionSpawnType, string>> MinionModels { get; }
        Dictionary<LaneID, List<Vector2>> MinionPaths { get; }
        Tuple<int, List<MinionSpawnType>> MinionWaveToSpawn(float gameTime, int cannonMinionCount, bool isInhibitorDead, bool areAllInhibitorsDead);
        TurretType GetTurretType(int trueIndex, LaneID lane, TeamId teamId);
        void Init(IMapScriptHandler map);
        void OnMatchStart();
        void SpawnAllCamps();
    }
}
using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace MapScripts.Map8
{
    public class ASCENSION : IMapScript
    {
        public IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            MinionSpawnEnabled = false,
            StartingGold = 1300.0f,
            OverrideSpawnPoints = true,
            RecallSpellItemId = 2005,
            GoldPerSecond = 9.85f,
            FirstGoldTime = 5 * 1000,
            InitialLevel = 3,
            ExpRange = 1250.0f,
            GoldRange = 0.0f,
            NavGridOverride = "AIPathASCENSION",
            ExpCurveOverride = "ExpCurveASCENSION"
        };

        public virtual IGlobalData GlobalData { get; set; } = new GlobalData();
        public bool HasFirstBloodHappened { get; set; } = false;
        public long NextSpawnTime { get; set; } = 90 * 1000;
        public string LaneMinionAI { get; set; } = "LaneMinionAI";

        //Values i got the values for 5 players from replay packets, the value for 1 player is just a guess of mine by using !coords command in-game
        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; } = new Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<int, Dictionary<int, Vector2>>{
                { 5, new Dictionary<int, Vector2>{
                    { 1, new Vector2(687.99036f, 4281.2314f) },
                    { 2, new Vector2(687.99036f, 4061.2314f) },
                    { 3, new Vector2(478.79034f,3993.2314f) },
                    { 4, new Vector2(349.39032f,4171.2314f) },
                    { 5, new Vector2(438.79034f,4349.2314f) }
                }},
                {1, new Dictionary<int, Vector2>{
                    { 1, new Vector2(580f, 4124f) }
                }}
            }},

            {TeamId.TEAM_PURPLE, new Dictionary<int, Dictionary<int, Vector2>>
                { { 5, new Dictionary<int, Vector2>{
                    { 1, new Vector2(13468.365f,4281.2324f) },
                    { 2, new Vector2(13468.365f,4061.2324f) },
                    { 3, new Vector2(13259.165f,3993.2324f) },
                    { 4, new Vector2(13129.765f,4171.2324f) },
                    { 5, new Vector2(13219.165f,4349.2324f) }
                }},
                {1, new Dictionary<int, Vector2>{
                    { 1, new Vector2(13310f, 4124f) }
                }}
            }},

        };

        //Minion models for this map
        public Dictionary<TeamId, Dictionary<MinionSpawnType, string>> MinionModels { get; set; } = new Dictionary<TeamId, Dictionary<MinionSpawnType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<MinionSpawnType, string>{
                {MinionSpawnType.MINION_TYPE_MELEE, "Blue_Minion_Basic"},
                {MinionSpawnType.MINION_TYPE_CASTER, "Blue_Minion_Wizard"},
                {MinionSpawnType.MINION_TYPE_CANNON, "Blue_Minion_MechCannon"},
                {MinionSpawnType.MINION_TYPE_SUPER, "Blue_Minion_MechMelee"}
            }},
            {TeamId.TEAM_PURPLE, new Dictionary<MinionSpawnType, string>{
                {MinionSpawnType.MINION_TYPE_MELEE, "Red_Minion_Basic"},
                {MinionSpawnType.MINION_TYPE_CASTER, "Red_Minion_Wizard"},
                {MinionSpawnType.MINION_TYPE_CANNON, "Red_Minion_MechCannon"},
                {MinionSpawnType.MINION_TYPE_SUPER, "Red_Minion_MechMelee"}
            }}
        };

        public void Init(Dictionary<GameObjectTypes, List<MapObject>> mapObjects)
        {
            //TODO: Implement Dynamic Minion spawn mechanics for Map8
            //SpawnEnabled = map.IsMinionSpawnEnabled();
            AddSurrender(1200000.0f, 300000.0f, 30.0f);
            CreateLevelProps.CreateProps();
            LevelScriptObjectsAscension.LoadObjects(mapObjects);
        }

        Dictionary<TeamId, int> TeamScores = new Dictionary<TeamId, int> { { TeamId.TEAM_BLUE, 200}, { TeamId.TEAM_PURPLE, 200} };
        public void OnMatchStart()
        {
            LevelScriptObjectsAscension.OnMatchStart();
            NeutralMinionSpawnAscension.InitializeNeutrals();

            foreach(var team in TeamScores.Keys)
            {
                NotifyGameScore(team, TeamScores[team]);
            }
        }

        public void Update(float diff)
        {
            NeutralMinionSpawnAscension.OnUpdate(diff);
        }

        public void SpawnAllCamps()
        {
            NeutralMinionSpawnAscension.ForceCampSpawn();
        }

        public Vector2 GetFountainPosition(TeamId team)
        {
            return LevelScriptObjects.FountainList[team].Position;
        }
    }
}
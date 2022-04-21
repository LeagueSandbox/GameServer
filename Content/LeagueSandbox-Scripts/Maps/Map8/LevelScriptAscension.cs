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
using LeagueSandbox.GameServer.API;
using System.Linq;

namespace MapScripts.Map8
{
    public class ASCENSION : IMapScript
    {
        public IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            MinionSpawnEnabled = false,
            OverrideSpawnPoints = true,
            RecallSpellItemId = 2007,
            BaseGoldPerGoldTick = 5.0f,
            InitialLevel = 3,
            AIVars = new AIVars
            {
                GoldRadius = 0.0f,
                StartingGold = 1300.0f,
                AmbientGoldDelay = 45.0f
            },
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

        Dictionary<TeamId, float> TeamScores = new Dictionary<TeamId, float> { { TeamId.TEAM_BLUE, 0.0f }, { TeamId.TEAM_PURPLE, 0.0f } };
        public void OnMatchStart()
        {
            LevelScriptObjectsAscension.OnMatchStart();
            NeutralMinionSpawnAscension.InitializeNeutrals();

            foreach (var team in TeamScores.Keys)
            {
                NotifyGameScore(team, TeamScores[team]);
            }

            AddParticle(null, null, "Odin_Forcefield_blue", new Vector2(580f, 4124f), -1);
            AddParticle(null, null, "Odin_Forcefield_purple", new Vector2(13310f, 4124f), -1);

            AddPosPerceptionBubble(new Vector2(6930.0f, 6443.0f), 550.0f, 25000, TeamId.TEAM_BLUE);
            AddPosPerceptionBubble(new Vector2(6930.0f, 6443.0f), 550.0f, 25000, TeamId.TEAM_PURPLE);

            NotifyWorldEvent(EventID.OnClearAscended);
            NotifyAscendant();

            foreach (var champion in GetAllChampions())
            {
                ApiEventManager.OnIncrementChampionScore.AddListener(this, champion, OnIncrementPoints, false);
                ApiEventManager.OnKill.AddListener(this, champion, OnChampionKill, false);
                AddBuff("AscRespawn", 25000.0f, 1, null, champion, null);
                AddBuff("AscHardModeEvent", 25000.0f, 1, null, champion, null);
            }

            foreach (var player in GetAllPlayers())
            {
                player.Inventory.AddItem(GetItemData(3460), player);
            }
        }

        public void Update(float diff)
        {
            NeutralMinionSpawnAscension.OnUpdate(diff);
            if (!allAnnouncementsAnnounced)
            {
                Announcements(GameTime());
            }
        }

        void OnIncrementPoints(IScoreData scoreData)
        {
            var owner = scoreData.Owner;
            var team = owner.Team;

            TeamScores[team] += scoreData.Points;
            NotifyGameScore(team, TeamScores[team]);

            if(TeamScores[team] >= 200)
            {
                foreach(var player in GetAllPlayersFromTeam(team))
                {
                    AddBuff("AscRespawn", 5.7f, 1, null, player, player);
                }

                var losingTeam = TeamId.TEAM_BLUE;
                if(team == TeamId.TEAM_BLUE)
                {
                    losingTeam = TeamId.TEAM_PURPLE;
                }
                EndGame(losingTeam, new Vector3(owner.Position.X, owner.GetHeight(), owner.Position.Y), 10000.0f, true, 2.0f);
            }
        }

        public void OnChampionKill(IDeathData data)
        {
            var killer = data.Killer as IChampion;

            killer.IncrementScore(1.0f, ScoreCategory.Combat, ScoreEvent.ChampKill, true);
        }

        public void SpawnAllCamps()
        {
            NeutralMinionSpawnAscension.ForceCampSpawn();
        }

        public Vector2 GetFountainPosition(TeamId team)
        {
            return LevelScriptObjectsAscension.FountainList[team].Position;
        }

        float notificationCounter = 0;
        bool allAnnouncementsAnnounced = false;
        public void Announcements(float gametime)
        {
            if(gametime >= 90.0f * 1000 && notificationCounter == 2)
            {
                NotifyWorldEvent(EventID.OnNexusCrystalStart);
                allAnnouncementsAnnounced=true;
            }
            else if(gametime >= 45.5f * 1000 && notificationCounter == 1)
            {
                NotifyWorldEvent(EventID.OnStartGameMessage2, 8);
                notificationCounter++;

            }
            else if(gametime >= 15.5f * 1000 && notificationCounter == 0)
            {
                NotifyWorldEvent(EventID.OnStartGameMessage1, 8);
                notificationCounter++;
            }
        }
    }
}
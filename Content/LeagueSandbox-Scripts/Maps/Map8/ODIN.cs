using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace MapScripts.Map8
{
    public class ODIN : IMapScript
    {
        public IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            MinionSpawnEnabled = false,
            StartingGold = 1375.0f,
            OverrideSpawnPoints = true,
            RecallSpellItemId = 2005,
            GoldPerSecond = 5.6f,
            FirstGoldTime = 90.0f * 1000,
            InitialLevel = 3
        };

        private bool forceSpawn;
        private IMapScriptHandler _map;
        private bool crystalSpawned;

        public virtual IGlobalData GlobalData { get; set; } = new GlobalData();
        public bool HasFirstBloodHappened { get; set; } = false;
        public long NextSpawnTime { get; set; } = 90 * 1000;
        public string LaneMinionAI { get; set; } = "LaneMinionAI";
        public string LaneTurretAI { get; set; } = "TurretAI";

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

        //Tower type enumeration might vary slightly from map to map, so we set that up here
        public TurretType GetTurretType(int trueIndex, LaneID lane, TeamId teamId)
        {
            return TurretType.NEXUS_TURRET;
        }

        //Nexus models
        //Nexus and Inhibitor model changes dont seem to take effect in-game, has to be investigated.
        public Dictionary<TeamId, string> NexusModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "OrderNexus" },
            {TeamId.TEAM_PURPLE, "ChaosNexus" }
        };
        //Inhib models
        public Dictionary<TeamId, string> InhibitorModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "OrderInhibitor" },
            {TeamId.TEAM_PURPLE, "ChaosInhibitor" }
        };
        //Tower Models
        public Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; set; } = new Dictionary<TeamId, Dictionary<TurretType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "OdinOrderTurretShrine" },
            } },
            {TeamId.TEAM_PURPLE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "OdinChaosTurretShrine" },
            } }
        };

        //Turret Items
        public Dictionary<TurretType, int[]> TurretItems { get; set; } = new Dictionary<TurretType, int[]>
        {
            { TurretType.OUTER_TURRET, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.INNER_TURRET, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.INHIBITOR_TURRET, new[] { 1501, 1502, 1503, 1505 } },
            { TurretType.NEXUS_TURRET, new[] { 1501, 1502, 1503, 1505 } }
        };

        //List of every path minions will take, separated by team and lane
        public Dictionary<LaneID, List<Vector2>> MinionPaths { get; set; }

        //List of every wave type
        public Dictionary<string, List<MinionSpawnType>> MinionWaveTypes = new Dictionary<string, List<MinionSpawnType>>
        { {"RegularMinionWave", new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER }
        },
        {"CannonMinionWave", new List<MinionSpawnType>{
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CANNON,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER }
        },
        {"SuperMinionWave", new List<MinionSpawnType>{
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER }
        },
        {"DoubleSuperMinionWave", new List<MinionSpawnType>{
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER }
        }
        };

        //Here you setup the conditions of which wave will be spawned
        public Tuple<int, List<MinionSpawnType>> MinionWaveToSpawn(float gameTime, int cannonMinionCount, bool isInhibitorDead, bool areAllInhibitorsDead)
        {
            var cannonMinionTimestamps = new List<Tuple<long, int>>
            {
                new Tuple<long, int>(0, 2),
                new Tuple<long, int>(20 * 60 * 1000, 1),
                new Tuple<long, int>(35 * 60 * 1000, 0)
            };
            var cannonMinionCap = 2;

            foreach (var timestamp in cannonMinionTimestamps)
            {
                if (gameTime >= timestamp.Item1)
                {
                    cannonMinionCap = timestamp.Item2;
                }
            }
            var list = "RegularMinionWave";
            if (cannonMinionCount >= cannonMinionCap)
            {
                list = "CannonMinionWave";
            }

            if (isInhibitorDead)
            {
                list = "SuperMinionWave";
            }

            if (areAllInhibitorsDead)
            {
                list = "DoubleSuperMinionWave";
            }
            return new Tuple<int, List<MinionSpawnType>>(cannonMinionCap, MinionWaveTypes[list]);
        }

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

        Dictionary<TeamId, ILevelProp> TeamStairs = new Dictionary<TeamId, ILevelProp>();
        Dictionary<TeamId, ILevelProp> Nexus = new Dictionary<TeamId, ILevelProp>();
        Dictionary<int, ILevelProp> SwainBeams = new Dictionary<int, ILevelProp>();
        //This function is executed in-between Loading the map structures and applying the structure protections. Is the first thing on this script to be executed
        public void Init(IMapScriptHandler map)
        {
            _map = map;

            //TODO: Implement Dynamic Minion spawn mechanics for Map8
            //SpawnEnabled = map.IsMinionSpawnEnabled();
            map.AddSurrender(1200000.0f, 300000.0f, 30.0f);

            map.AddLevelProp("LevelProp_Odin_Windmill_Gears", "Odin_Windmill_Gears", new Vector2(6946.143f, 11918.931f), -122.93308f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(11.1111f, 77.7777f, -122.2222f), Vector3.One);
            map.AddLevelProp("LevelProp_Odin_Windmill_Propellers", "Odin_Windmill_Propellers", new Vector2(6922.032f, 11940.535f), -259.16052f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-22.2222f, 0.0f, -111.1111f), Vector3.One);
            map.AddLevelProp("LevelProp_Odin_Lifts_Buckets", "Odin_Lifts_Buckets", new Vector2(2123.782f, 8465.207f), -122.9331f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(188.8889f, 77.7777f, 444.4445f), Vector3.One);
            map.AddLevelProp("LevelProp_Odin_Lifts_Crystal", "Odin_Lifts_Crystal", new Vector2(1578.0967f, 7505.5938f), -78.48851f, new Vector3(0.0f, 12.0f, 0.0f), new Vector3(-233.3335f, 100.0f, -544.4445f), Vector3.One);
            map.AddLevelProp("LevelProp_OdinRockSaw02", "OdinRockSaw", new Vector2(5659.9004f, 1016.47925f), -11.821701f, new Vector3(0.0f, 40.0f, 0.0f), new Vector3(233.3334f, 133.3334f, -77.7778f), Vector3.One);
            map.AddLevelProp("LevelProp_OdinRockSaw01", "OdinRockSaw", new Vector2(2543.822f, 1344.957f), -56.266106f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-122.2222f, 111.1112f, -744.4445f), Vector3.One);
            map.AddLevelProp("LevelProp_Odin_Drill", "Odin_Drill", new Vector2(11992.028f, 8547.805f), 343.7337f, new Vector3(0.0f, 244.0f, 0.0f), new Vector3(33.3333f, 311.1111f, 0.0f), Vector3.One);
            TeamStairs.Add(TeamId.TEAM_BLUE, map.AddLevelProp("LevelProp_Odin_SoG_Order", "Odin_SoG_Order", new Vector2(266.77225f, 3903.9998f), 139.9266f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-288.8889f, 122.2222f, -188.8889f), Vector3.One));
            map.AddLevelProp("LevelProp_OdinClaw", "OdinClaw", new Vector2(5187.914f, 2122.2627f), 261.546f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(422.2223f, 255.5555f, -200.0f), Vector3.One);
            SwainBeams.Add(1, map.AddLevelProp("LevelProp_SwainBeam1", "SwainBeam", new Vector2(7207.073f, 1995.804f), 461.54602f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-422.2222f, 355.5555f, -311.1111f), Vector3.One));
            SwainBeams.Add(2, map.AddLevelProp("LevelProp_SwainBeam2", "SwainBeam", new Vector2(8142.406f, 2716.4258f), 639.324f, new Vector3(0.0f, 152.0f, 0.0f), new Vector3(-222.2222f, 444.4445f, -88.8889f), Vector3.One));
            SwainBeams.Add(3, map.AddLevelProp("LevelProp_SwainBeam3", "SwainBeam", new Vector2(9885.076f, 3339.1853f), 350.435f, new Vector3(0.0f, 54.0f, 0.0f), new Vector3(144.4445f, 300.0f, -155.5555f), Vector3.One));
            TeamStairs.Add(TeamId.TEAM_PURPLE, map.AddLevelProp("LevelProp_Odin_SoG_Chaos", "Odin_SoG_Chaos", new Vector2(13623.644f, 3884.6233f), 117.7046f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(288.8889f, 111.1112f, -211.1111f), Vector3.One));
            map.AddLevelProp("LevelProp_OdinCrane", "OdinCrane", new Vector2(10287.527f, 10776.917f), -145.15509f, new Vector3(0.0f, 52.0f, 0.0f), new Vector3(-22.2222f, 66.6667f, 0.0f), Vector3.One);
            map.AddLevelProp("LevelProp_OdinCrane1", "OdinCrane", new Vector2(9418.097f, -189.59952f), 12105.366f, new Vector3(0.0f, 118.0f, 0.0f), new Vector3(0.0f, 44.4445f, 0.0f), Vector3.One);
            Nexus.Add(TeamId.TEAM_BLUE, map.AddLevelProp("LevelProp_Odin_SOG_Order_Crystal", "Odin_SOG_Order_Crystal", new Vector2(1618.3121f, 4357.871f), 336.9458f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-122.2222f, 277.7778f, -122.2222f), Vector3.One));
            Nexus.Add(TeamId.TEAM_PURPLE, map.AddLevelProp("LevelProp_Odin_SOG_Chaos_Crystal", "Odin_SOG_Chaos_Crystal", new Vector2(12307.629f, 4535.6484f), 225.8346f, new Vector3(0.0f, 214.0f, 0.0f), new Vector3(144.4445f, 222.2222f, -33.3334f), Vector3.One));
        }

        List<IMinion> InfoPoints = new List<IMinion>();
        List<IMonsterCamp> SpeedShrines = new List<IMonsterCamp>();
        List<IMonsterCamp> HealthPacks = new List<IMonsterCamp>();

        //Since the center crystals are treated as simple minions intead of camp/monster, we have to hand everything individually
        Dictionary<TeamId, IMinion> Crystals = new Dictionary<TeamId, IMinion>();
        List<MinionTemplate> CrystalsTemplates = new List<MinionTemplate>();
        Dictionary<TeamId, float> CrystalTimers = new Dictionary<TeamId, float> { { TeamId.TEAM_BLUE, 180.0f * 1000 }, { TeamId.TEAM_PURPLE, 180.0f * 1000 } };
        Dictionary<TeamId, List<IRegion>> CrystalRegions = new Dictionary<TeamId, List<IRegion>> { { TeamId.TEAM_BLUE, new List<IRegion>() }, { TeamId.TEAM_PURPLE, new List<IRegion>() } };
        public void OnMatchStart()
        {
            for (int i = 0; i < _map.InfoPoints.Count; i++)
            {
                var point = _map.CreateMinion("OdinNeutralGuardian", "OdinNeutralGuardian", new Vector2(_map.InfoPoints[i].CentralPoint.X, _map.InfoPoints[i].CentralPoint.Z), ignoreCollision: true);
                InfoPoints.Add(point);
                AddUnitPerceptionBubble(point, 800.0f, 25000.0f, TeamId.TEAM_BLUE, true, collisionArea: 120.0f, collisionOwner: point);
                InfoPoints[i].PauseAi(true);
            }

            SetupCamps();

            CrystalsTemplates.Add(new MinionTemplate(null, "OdinCenterRelic", "OdinCenterRelic", new Vector2(7074.9736f, 6462.0273f), team: TeamId.TEAM_BLUE));
            CrystalsTemplates.Add(new MinionTemplate(null, "OdinCenterRelic", "OdinCenterRelic", new Vector2(6801.1855f, 6462.0273f), team: TeamId.TEAM_PURPLE));

            foreach (var camp in SpeedShrines)
            {
                if (!camp.IsAlive)
                {
                    AddPosPerceptionBubble(new Vector2(camp.Position.X, camp.Position.Z), 250.0f, 1.0f, TeamId.TEAM_BLUE);
                    AddPosPerceptionBubble(new Vector2(camp.Position.X, camp.Position.Z), 250.0f, 1.0f, TeamId.TEAM_PURPLE);
                    _map.SpawnCamp(camp);
                }
            }

            AddParticleTarget(Nexus[TeamId.TEAM_BLUE], Nexus[TeamId.TEAM_BLUE], "Odin_Crystal_blue", Nexus[TeamId.TEAM_BLUE], 25000);
            AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], Nexus[TeamId.TEAM_PURPLE], "Odin_Crystal_purple", Nexus[TeamId.TEAM_PURPLE], 25000);

            AddParticle(null, null, "Odin_Forcefield_blue", new Vector2(580f, 4124f), 80.0f);
            AddParticle(null, null, "Odin_Forcefield_purple", new Vector2(13310f, 4124f), 80.0f);

            foreach (var stair in TeamStairs.Values)
            {
                _map.NotifyPropAnimation(stair, "Open", (AnimationFlags)1, 0.0f, false);
            }
            _map.NotifyPropAnimation(SwainBeams[1], "PeckA", (AnimationFlags)1, 0.0f, false);
            _map.NotifyPropAnimation(SwainBeams[2], "PeckB", (AnimationFlags)1, 0.0f, false);
            _map.NotifyPropAnimation(SwainBeams[3], "PeckC", (AnimationFlags)1, 0.0f, false);

            foreach (var champion in GetAllPlayers())
            {
                AddBuff("OdinPlayerBuff", 25000, 1, null, champion, null);
            }
        }

        public void Update(float diff)
        {
            if (crystalSpawned)
            {
                foreach (var crystal in Crystals.Values)
                {
                    string iconCategory = "CenterRelicLeft";

                    if (crystal.Team == TeamId.TEAM_PURPLE)
                    {
                        iconCategory = "CenterRelicRight";
                    }

                    //For some Reason this only works here
                    _map.SetMinimapIcon(crystal, iconCategory, true);
                }

                crystalSpawned = false;
            }

            foreach (var camp in HealthPacks)
            {
                if (!camp.IsAlive)
                {
                    camp.RespawnTimer -= diff;
                    if (camp.RespawnTimer <= 0 || forceSpawn)
                    {
                        _map.SpawnCamp(camp);
                        camp.RespawnTimer = 30.0f * 1000f;
                    }
                }
            }

            foreach (var crystalTemplate in CrystalsTemplates)
            {
                if (!Crystals.ContainsKey(crystalTemplate.Team))
                {
                    CrystalTimers[crystalTemplate.Team] -= diff;

                    if (CrystalTimers[crystalTemplate.Team] <= 0 || forceSpawn)
                    {
                        var crystal = _map.CreateMinion(crystalTemplate.Name, crystalTemplate.Model, crystalTemplate.Position,
                                crystalTemplate.NetId, crystalTemplate.Team, crystalTemplate.SkinId,
                                crystalTemplate.IgnoresCollision, crystalTemplate.IsTargetable);

                        AddUnitPerceptionBubble(crystal, 350.0f, 25000.0f, TeamId.TEAM_BLUE, collisionArea: 38.08f, collisionOwner: crystal);
                        AddUnitPerceptionBubble(crystal, 350.0f, 25000.0f, TeamId.TEAM_PURPLE, collisionArea: 38.08f, collisionOwner: crystal);

                        ApiEventManager.OnDeath.AddListener(crystal, crystal, OnCrystalDeath, true);

                        Crystals.Add(crystal.Team, crystal);
                        CrystalTimers[crystalTemplate.Team] = 180.0f * 1000f;

                        crystalSpawned = true;
                    }
                }
            }

            var gameTime = _map.GameTime();

            if (!NotifiedAllInitialAnimations)
            {
                InitialBaseAnimations(gameTime);
            }

            if (!AllAnnouncementsAnnounced)
            {
                CheckInitialMapAnnouncements(gameTime);
            }

            if (forceSpawn)
            {
                forceSpawn = false;
            }
        }

        public void OnCrystalDeath(IDeathData deathData)
        {
            Crystals.Remove(deathData.Unit.Team);
            foreach (var region in CrystalRegions[deathData.Unit.Team])
            {
                region.SetToRemove();
            }
        }

        public void SpawnAllCamps()
        {
            forceSpawn = true;
        }

        public float GetGoldFor(IAttackableUnit u)
        {
            if (!(u is ILaneMinion m))
            {
                if (!(u is IChampion c))
                {
                    return 0.0f;
                }

                var gold = 300.0f; //normal gold for a kill
                if (c.KillDeathCounter < 5 && c.KillDeathCounter >= 0)
                {
                    if (c.KillDeathCounter == 0)
                    {
                        return gold;
                    }

                    for (var i = c.KillDeathCounter; i > 1; --i)
                    {
                        gold += gold * 0.165f;
                    }

                    return gold;
                }

                if (c.KillDeathCounter >= 5)
                {
                    return 500.0f;
                }

                if (c.KillDeathCounter >= 0)
                    return 0.0f;

                var firstDeathGold = gold - gold * 0.085f;

                if (c.KillDeathCounter == -1)
                {
                    return firstDeathGold;
                }

                for (var i = c.KillDeathCounter; i < -1; ++i)
                {
                    firstDeathGold -= firstDeathGold * 0.2f;
                }

                if (firstDeathGold < 50)
                {
                    firstDeathGold = 50;
                }

                return firstDeathGold;
            }

            var dic = new Dictionary<MinionSpawnType, float>
            {
                { MinionSpawnType.MINION_TYPE_MELEE, 19.8f + 0.2f * (int)(_map.GameTime() / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CASTER, 16.8f + 0.2f * (int)(_map.GameTime() / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CANNON, 40.0f + 0.5f * (int)(_map.GameTime() / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_SUPER, 40.0f + 1.0f * (int)(_map.GameTime() / (180 * 1000)) }
            };

            if (!dic.ContainsKey(m.MinionSpawnType))
            {
                return 0.0f;
            }

            return dic[m.MinionSpawnType];
        }

        public float GetExperienceFor(IAttackableUnit u)
        {
            if (!(u is ILaneMinion m))
            {
                return 0.0f;
            }

            var dic = new Dictionary<MinionSpawnType, float>
            {
                { MinionSpawnType.MINION_TYPE_MELEE, 64.0f },
                { MinionSpawnType.MINION_TYPE_CASTER, 32.0f },
                { MinionSpawnType.MINION_TYPE_CANNON, 92.0f },
                { MinionSpawnType.MINION_TYPE_SUPER, 97.0f }
            };

            if (!dic.ContainsKey(m.MinionSpawnType))
            {
                return 0.0f;
            }

            return dic[m.MinionSpawnType];
        }

        public void SetMinionStats(ILaneMinion m)
        {
            // Same for all minions
            m.Stats.MoveSpeed.BaseValue = 325.0f;

            switch (m.MinionSpawnType)
            {
                case MinionSpawnType.MINION_TYPE_MELEE:
                    m.Stats.CurrentHealth = 475.0f + 20.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.HealthPoints.BaseValue = 475.0f + 20.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.AttackDamage.BaseValue = 12.0f + 1.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.Range.BaseValue = 180.0f;
                    m.Stats.AttackSpeedFlat = 1.250f;
                    m.IsMelee = true;
                    break;
                case MinionSpawnType.MINION_TYPE_CASTER:
                    m.Stats.CurrentHealth = 279.0f + 7.5f * (int)(_map.GameTime() / (90 * 1000));
                    m.Stats.HealthPoints.BaseValue = 279.0f + 7.5f * (int)(_map.GameTime() / (90 * 1000));
                    m.Stats.AttackDamage.BaseValue = 23.0f + 1.0f * (int)(_map.GameTime() / (90 * 1000));
                    m.Stats.Range.BaseValue = 600.0f;
                    m.Stats.AttackSpeedFlat = 0.670f;
                    break;
                case MinionSpawnType.MINION_TYPE_CANNON:
                    m.Stats.CurrentHealth = 700.0f + 27.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.HealthPoints.BaseValue = 700.0f + 27.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.AttackDamage.BaseValue = 40.0f + 3.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.Range.BaseValue = 450.0f;
                    m.Stats.AttackSpeedFlat = 1.0f;
                    break;
                case MinionSpawnType.MINION_TYPE_SUPER:
                    m.Stats.CurrentHealth = 1500.0f + 200.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.HealthPoints.BaseValue = 1500.0f + 200.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.AttackDamage.BaseValue = 190.0f + 10.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.Range.BaseValue = 170.0f;
                    m.Stats.AttackSpeedFlat = 0.694f;
                    m.Stats.Armor.BaseValue = 30.0f;
                    m.Stats.MagicResist.BaseValue = -30.0f;
                    m.IsMelee = true;
                    break;
            }
        }

        bool AllAnnouncementsAnnounced = false;
        List<EventID> AnnouncedEvents = new List<EventID>();
        public void CheckInitialMapAnnouncements(float time)
        {
            if (time >= 90.0f * 1000)
            {
                _map.NotifyMapAnnouncement(EventID.OnNexusCrystalStart, 0);
                AllAnnouncementsAnnounced = true;

            }
            if (time >= 80.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage2))
            {
                // The Battle Has Beguns!
                _map.NotifyMapAnnouncement(EventID.OnStartGameMessage2, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage2);
            }
            else if (time >= 50.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage1))
            {
                // The battle will begin in 30 seconds!
                _map.NotifyMapAnnouncement(EventID.OnStartGameMessage1, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage1);
            }
            else if (time >= 30.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage3))
            {
                // Welcome to the Crystal Scar!
                _map.NotifyMapAnnouncement(EventID.OnStartGameMessage3, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage3);
            }
        }

        List<string> AnimationsNotified = new List<string>();
        bool NotifiedAllInitialAnimations = false;
        public void InitialBaseAnimations(float gameTime)
        {
            if (gameTime >= 87.0f * 1000 && !AnimationsNotified.Contains("Raised_Idle"))
            {
                foreach (var stair in TeamStairs.Values)
                {
                    _map.NotifyPropAnimation(stair, "Raised_Idle", (AnimationFlags)1, 4.25f, false);
                }
                NotifiedAllInitialAnimations = true;
            }
            else if (gameTime >= 81.0f * 1000 && !AnimationsNotified.Contains("Raise"))
            {
                foreach (var stair in TeamStairs.Values)
                {
                    _map.NotifyPropAnimation(stair, "Raise", (AnimationFlags)2, 6.7f, false);
                }
                AnimationsNotified.Add("Raise");
            }
            else if (gameTime >= 80.0f * 1000 && !AnimationsNotified.Contains("Particles4"))
            {
                //Blue Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_l_1_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_r_1_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_r_1_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_l_1_aim", teamOnly: TeamId.TEAM_PURPLE);

                //Purple Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_1_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_1_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_1_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_1_aim", teamOnly: TeamId.TEAM_BLUE);

                AnimationsNotified.Add("Particles4");
            }
            else if (gameTime >= 61.0f * 1000 && !AnimationsNotified.Contains("Close4"))
            {
                foreach (var stair in TeamStairs.Values)
                {
                    _map.NotifyPropAnimation(stair, "Close4", (AnimationFlags)2, 20.0f, false);
                }
                AnimationsNotified.Add("Close4");
            }
            else if (gameTime >= 59.6f * 1000 && !AnimationsNotified.Contains("Particles3"))
            {
                //Blue Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_l_2_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_r_2_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_l_2_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_r_2_aim", teamOnly: TeamId.TEAM_PURPLE);

                //Purple Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_2_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_2_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_2_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_2_aim", teamOnly: TeamId.TEAM_BLUE);

                AnimationsNotified.Add("Particles3");
            }
            else if (gameTime >= 41.0f * 1000 && !AnimationsNotified.Contains("Close3"))
            {
                foreach (var stair in TeamStairs.Values)
                {
                    _map.NotifyPropAnimation(stair, "Close3", (AnimationFlags)2, 20.0f, false);
                }
                AnimationsNotified.Add("Close3");
            }
            else if (gameTime >= 40.0f * 1000 && !AnimationsNotified.Contains("Particles2"))
            {
                //Blue Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000, 1, "Crystal_l_3_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000, 1, "Crystal_r_3_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000, 1, "Crystal_l_3_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000, 1, "Crystal_r_3_aim", teamOnly: TeamId.TEAM_PURPLE);

                //Purple Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_3_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_3_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_3_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_3_aim", teamOnly: TeamId.TEAM_BLUE);

                AnimationsNotified.Add("Particles2");
            }
            else if (gameTime >= 21.0f * 1000 && !AnimationsNotified.Contains("Close2"))
            {
                foreach (var stair in TeamStairs.Values)
                {
                    _map.NotifyPropAnimation(stair, "Close2", (AnimationFlags)2, 20.0f, false);
                }
                AnimationsNotified.Add("Close2");
            }
            //The timing feels off, but from what i've seen from old footage, it looks like it is just like that
            else if (gameTime >= 16.0f * 1000 && !AnimationsNotified.Contains("Particles1"))
            {
                //Blue Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_l_4_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_r_4_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_l_4_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_r_4_aim", teamOnly: TeamId.TEAM_PURPLE);

                //Purple Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_4_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_4_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_4_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_4_aim", teamOnly: TeamId.TEAM_BLUE);

                //Center Crystal Particles
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], Nexus[TeamId.TEAM_BLUE], "Odin_crystal_beam_hit_blue", Nexus[TeamId.TEAM_BLUE], 25000.0f, 2);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], Nexus[TeamId.TEAM_PURPLE], "Odin_crystal_beam_hit_purple", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 2);

                AnimationsNotified.Add("Particles1");
            }
            else if (gameTime >= 1000.0f && !AnimationsNotified.Contains("Close1"))
            {
                foreach (var stair in TeamStairs.Values)
                {
                    _map.NotifyPropAnimation(stair, "Close1", (AnimationFlags)2, 17.5f, false);
                }
                AnimationsNotified.Add("Close1");
            }
        }

        public void SetupCamps()
        {
            var speedShrine1 = _map.CreateJungleCamp(new Vector3(5022.9287f, 60.0f, 7778.2695f), 102, 0, "Shrine", 0);
            _map.CreateJungleMonster("OdinSpeedShrine", "OdinSpeedShrine", new Vector2(5022.9287f, 7778.2695f), new Vector3(-0.0f, 0.0f, 1.0f), speedShrine1, isTargetable: false, ignoresCollision: true);
            SpeedShrines.Add(speedShrine1);

            var speedShrine2 = _map.CreateJungleCamp(new Vector3(8859.897f, 60.0f, 7788.1064f), 103, 0, "Shrine", 0);
            _map.CreateJungleMonster("OdinSpeedShrine", "OdinSpeedShrine", new Vector2(8859.897f, 7788.1064f), new Vector3(-0.0f, 0.0f, 1.0f), speedShrine2, isTargetable: false, ignoresCollision: true);
            SpeedShrines.Add(speedShrine2);

            var speedShrine3 = _map.CreateJungleCamp(new Vector3(6962.6934f, 60.0f, 4089.48f), 104, 0, "Shrine", 0);
            _map.CreateJungleMonster("OdinSpeedShrine", "OdinSpeedShrine", new Vector2(6962.6934f, 4089.48f), new Vector3(-0.0f, 0.0f, 1.0f), speedShrine3, isTargetable: false, ignoresCollision: true);
            SpeedShrines.Add(speedShrine3);



            var healthPacket1 = _map.CreateJungleCamp(new Vector3(4948.231f, 60.0f, 9329.905f), 100, 0, "HealthPack", 120.0f * 1000f);
            _map.CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(4948.231f, 9329.905f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket1);
            HealthPacks.Add(healthPacket1);

            var healthPacket2 = _map.CreateJungleCamp(new Vector3(8972.231f, 60.0f, 9329.905f), 101, 0, "HealthPack", 120.0f * 1000f);
            _map.CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(8972.231f, 9329.905f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket2);
            HealthPacks.Add(healthPacket2);

            var healthPacket3 = _map.CreateJungleCamp(new Vector3(6949.8193f, 60.0f, 2855.0513f), 112, 0, "HealthPack", 120.0f * 1000f);
            _map.CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(6949.8193f, 2855.0513f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket3);
            HealthPacks.Add(healthPacket3);

            var healthPacket4 = _map.CreateJungleCamp(new Vector3(6947.838f, 60.0f, 12116.367f), 108, 0, "HealthPack", 120.0f * 1000f);
            _map.CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(6947.838f, 12116.367f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket4);
            HealthPacks.Add(healthPacket4);

            var healthPacket5 = _map.CreateJungleCamp(new Vector3(12881.534f, 60.0f, 8294.764f), 109, 0, "HealthPack", 120.0f * 1000f);
            _map.CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(12881.534f, 8294.764f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket5);
            HealthPacks.Add(healthPacket5);

            var healthPacket6 = _map.CreateJungleCamp(new Vector3(10242.127f, 60.0f, 1519.5938f), 105, 0, "HealthPack", 120.0f * 1000f);
            _map.CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(10242.127f, 1519.5938f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket6);
            HealthPacks.Add(healthPacket6);

            var healthPacket7 = _map.CreateJungleCamp(new Vector3(3639.7327f, 60.0f, 1490.0762f), 106, 0, "HealthPack", 120.0f * 1000f);
            _map.CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(3639.7327f, 1490.0762f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket7);
            HealthPacks.Add(healthPacket7);

            var healthPacket8 = _map.CreateJungleCamp(new Vector3(1027.4365f, 60.0f, 8288.714f), 107, 0, "HealthPack", 120.0f * 1000f);
            _map.CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(1027.4365f, 8288.714f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket8);
            HealthPacks.Add(healthPacket8);

            var healthPacket9 = _map.CreateJungleCamp(new Vector3(4324.928f, 60.0f, 5500.919f), 110, 0, "HealthPack", 120.0f * 1000f);
            _map.CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(4324.928f, 5500.919f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket9);
            HealthPacks.Add(healthPacket9);

            var healthPacket10 = _map.CreateJungleCamp(new Vector3(9573.432f, 60.0f, 5530.13f), 111, 0, "HealthPack", 120.0f * 1000f);
            _map.CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(9573.432f, 5530.13f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket10);
            HealthPacks.Add(healthPacket10);
        }
    }
}

public class MinionTemplate
{
    public IObjAiBase Owner { get; set; }
    public string Name { get; set; }
    public string Model { get; set; }
    public Vector2 Position { get; set; }
    public int SkinId { get; set; }
    public TeamId Team { get; set; }
    public uint NetId { get; set; }
    public bool IsTargetable { get; set; }
    public bool IgnoresCollision { get; set; }
    public string AiScript { get; set; }
    public int DamageBonus { get; set; }
    public int HealthBonus { get; set; }
    public int InitialLevel { get; set; }
    public IObjAiBase VisibilityOwner { get; set; }

    public MinionTemplate(
        IObjAiBase owner,
        string model,
        string name,
        Vector2 position,
        uint netId = 0,
        TeamId team = TeamId.TEAM_NEUTRAL,
        int skinId = 0,
        bool ignoreCollision = false,
        bool targetable = true,
        IObjAiBase visibilityOwner = null,
        string aiScript = "",
        int damageBonus = 0,
        int healthBonus = 0,
        int initialLevel = 1
    )
    {
        Owner = owner;
        Name = name;
        Model = model;
        Team = team;
        Position = position;
        NetId = netId;
        IsTargetable = targetable;
        IgnoresCollision = ignoreCollision;
        AiScript = aiScript;
        DamageBonus = damageBonus;
        HealthBonus = healthBonus;
        InitialLevel = initialLevel;
        VisibilityOwner = visibilityOwner;
        SkinId = skinId;
    }
}
using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace MapScripts.Map8
{
    public class CLASSIC : IMapScript
    {
        public IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            MinionSpawnEnabled = false,
            StartingGold = 825.0f
        };
        public virtual IGlobalData GlobalData { get; set; } = new GlobalData();
        public bool HasFirstBloodHappened { get; set; } = false;
        public long NextSpawnTime { get; set; } = 90 * 1000;
        private IMapScriptHandler _map;

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

        //This function is executed in-between Loading the map structures and applying the structure protections. Is the first thing on this script to be executed
        public void Init(IMapScriptHandler map)
        {
            _map = map;

            //TODO: Implement Dynamic Minion spawn mechanics for Map8
            //SpawnEnabled = map.IsMinionSpawnEnabled();
            map.AddSurrender(1200000.0f, 300000.0f, 30.0f);

            // Announcer events
            map.AddAnnouncement(30 * 1000, EventID.OnStartGameMessage1, true); // Welcome to "Map"
            map.AddAnnouncement(NextSpawnTime - 30 * 1000, EventID.OnStartGameMessage2, true); // 30 seconds until minions spawn
            map.AddAnnouncement(NextSpawnTime, EventID.OnMinionsSpawn, false); // Minions have spawned

            map.AddLevelProp("LevelProp_Odin_Windmill_Gears", "Odin_Windmill_Gears", new Vector2(6946.143f, 11918.931f), -122.93308f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(11.1111f, 77.7777f, -122.2222f), Vector3.One);
            map.AddLevelProp("LevelProp_Odin_Windmill_Propellers", "Odin_Windmill_Propellers", new Vector2(6922.032f, 11940.535f), -259.16052f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-22.2222f, 0.0f, -111.1111f), Vector3.One);
            map.AddLevelProp("LevelProp_Odin_Lifts_Buckets", "Odin_Lifts_Buckets", new Vector2(2123.782f, 8465.207f), -122.9331f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(188.8889f, 77.7777f, 444.4445f), Vector3.One);
            map.AddLevelProp("LevelProp_Odin_Lifts_Crystal", "Odin_Lifts_Crystal", new Vector2(1578.0967f, 7505.5938f), -78.48851f, new Vector3(0.0f, 12.0f, 0.0f), new Vector3(-233.3335f, 100.0f, -544.4445f), Vector3.One);
            map.AddLevelProp("LevelProp_OdinRockSaw02", "OdinRockSaw", new Vector2(5659.9004f, 1016.47925f), -11.821701f, new Vector3(0.0f, 40.0f, 0.0f), new Vector3(233.3334f, 133.3334f, -77.7778f), Vector3.One);
            map.AddLevelProp("LevelProp_OdinRockSaw01", "OdinRockSaw", new Vector2(2543.822f, 1344.957f), -56.266106f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-122.2222f, 111.1112f, -744.4445f), Vector3.One);
            map.AddLevelProp("LevelProp_Odin_Drill", "Odin_Drill", new Vector2(11992.028f, 8547.805f), 343.7337f, new Vector3(0.0f, 244.0f, 0.0f), new Vector3(33.3333f, 311.1111f, 0.0f), Vector3.One);
            map.AddLevelProp("LevelProp_Odin_SoG_Order", "Odin_SoG_Order", new Vector2(266.77225f, 3903.9998f), 139.9266f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-288.8889f, 122.2222f, -188.8889f), Vector3.One);
            map.AddLevelProp("LevelProp_OdinClaw", "OdinClaw", new Vector2(5187.914f, 2122.2627f), 261.546f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(422.2223f, 255.5555f, -200.0f), Vector3.One);
            map.AddLevelProp("LevelProp_SwainBeam1", "SwainBeam", new Vector2(7207.073f, 1995.804f), 461.54602f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-422.2222f, 355.5555f, -311.1111f), Vector3.One);
            map.AddLevelProp("LevelProp_SwainBeam2", "SwainBeam", new Vector2(8142.406f, 2716.4258f), 639.324f, new Vector3(0.0f, 152.0f, 0.0f), new Vector3(-222.2222f, 444.4445f, -88.8889f), Vector3.One);
            map.AddLevelProp("LevelProp_SwainBeam3", "SwainBeam", new Vector2(9885.076f, 3339.1853f), 350.435f, new Vector3(0.0f, 54.0f, 0.0f), new Vector3(144.4445f, 300.0f, -155.5555f), Vector3.One);
            map.AddLevelProp("LevelProp_Odin_SoG_Chaos", "Odin_SoG_Chaos", new Vector2(13623.644f, 3884.6233f), 117.7046f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(288.8889f, 111.1112f, -211.1111f), Vector3.One);
            map.AddLevelProp("LevelProp_OdinCrane", "OdinCrane", new Vector2(10287.527f, 10776.917f), -145.15509f, new Vector3(0.0f, 52.0f, 0.0f), new Vector3(-22.2222f, 66.6667f, 0.0f), Vector3.One);
            map.AddLevelProp("LevelProp_OdinCrane1", "OdinCrane", new Vector2(9418.097f, -189.59952f), 12105.366f, new Vector3(0.0f, 118.0f, 0.0f), new Vector3(0.0f, 44.4445f, 0.0f), Vector3.One);
            map.AddLevelProp("LevelProp_Odin_SOG_Order_Crystal", "Odin_SOG_Order_Crystal", new Vector2(1618.3121f, 4357.871f), 336.9458f, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-122.2222f, 277.7778f, -122.2222f), Vector3.One);
            map.AddLevelProp("LevelProp_Odin_SOG_Chaos_Crystal", "Odin_SOG_Chaos_Crystal", new Vector2(12307.629f, 4535.6484f), 225.8346f, new Vector3(0.0f, 214.0f, 0.0f), new Vector3(144.4445f, 222.2222f, -33.3334f), Vector3.One);
        }

        List<IMinion> infoPoints = new List<IMinion>();
        public void OnMatchStart()
        {
            for( int i = 0; i < _map.InfoPoints.Count; i++)
            {
                _map.CreateRegion(TeamId.TEAM_BLUE, new Vector2(_map.InfoPoints[i].CentralPoint.X, _map.InfoPoints[i].CentralPoint.Z), RegionType.Unknown2, null, giveVision: true, visionRadius: 800.0f, revealStealth: true, hasCollision: true, collisionRadius: 120.0f, grassRadius: 150.0f, lifeTime: 25000.0f);
                infoPoints.Add(_map.CreateMinion("OdinNeutralGuardian", "OdinNeutralGuardian", new Vector2(_map.InfoPoints[i].CentralPoint.X, _map.InfoPoints[i].CentralPoint.Z), ignoreCollision: true));
                infoPoints[i].PauseAi(true);
            }
        }
        //This function gets executed every server tick
        public void Update(float diff)
        {
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
    }
}

using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerLib.GameObjects
{
    public class MonsterCamp : GameObject, IMonsterCamp
    {
        private TeamId[] _playerTeams = new TeamId[] { TeamId.TEAM_BLUE, TeamId.TEAM_PURPLE };
        private Dictionary<TeamId, bool> _teamSawLastDeath = new Dictionary<TeamId, bool>{
            { TeamId.TEAM_BLUE, true },
            { TeamId.TEAM_PURPLE, true },
        };
        private Dictionary<TeamId, bool> _isAliveForTeam = new Dictionary<TeamId, bool>{
            { TeamId.TEAM_BLUE, false },
            { TeamId.TEAM_PURPLE, false },
        };
        private Dictionary<int, bool> _isAliveForPlayer = new Dictionary<int, bool>();

        public byte CampIndex { get; set; }
        public new Vector3 Position { get; set; }
        public byte SideTeamId { get; set; }
        public string MinimapIcon { get; set; }
        public byte RevealEvent { get; set; }
        public float Expire { get; set; }
        public int TimerType { get; set; }
        public float SpawnDuration { get; set; }
        public float DoPlayVO { get; set; }
        public bool IsAlive { get; set; } = false;
        public float RespawnTimer { get; set; }
        public List<IMonster> Monsters { get; set; } = new List<IMonster>();

        public override bool IsAffectedByFoW => true;

        public MonsterCamp(
            Game game, Vector3 position, byte groupNumber, TeamId teamSideOfTheMap,
            string campTypeIcon, float respawnTimer, bool doPlayVO = true, byte revealEvent = 74,
            float spawnDuration = 0
        ): base(
            game, new Vector2(position.X, position.Z), 0, 0, 0, team: TeamId.TEAM_NEUTRAL
        )
        {
            Position = position;
            CampIndex = groupNumber;
            RevealEvent = revealEvent;
            MinimapIcon = campTypeIcon;
            RespawnTimer = respawnTimer;
            SideTeamId = (byte)teamSideOfTheMap;
            SpawnDuration = spawnDuration;
        }

        public override void LateUpdate(float diff)
        {
            foreach(TeamId team in _playerTeams)
            {
                if(IsVisibleByTeam(team))
                {
                    _isAliveForTeam[team] = IsAlive;
                }
            }
        }

        public override void Sync(int userId, TeamId team, bool visible, bool forceSpawn = false)
        {
            base.Sync(userId, team, visible, forceSpawn);

            bool isAliveForTeam = _isAliveForTeam[team];
            bool isAliveForPlayer = _isAliveForPlayer.GetValueOrDefault(userId, false);
            if
            (
                (forceSpawn && isAliveForPlayer) // Reconnect
                || (isAliveForPlayer != isAliveForTeam
                    // TODO: Handle based on vision radius, not status (also handle null peer info)
                    && !_game.PlayerManager.GetPeerInfo(userId).Champion.Status.HasFlag(StatusFlags.NearSighted))
            )
            {
                if(_isAliveForPlayer[userId] = isAliveForTeam)
                {
                    _game.PacketNotifier.NotifyS2C_ActivateMinionCamp(this, userId);
                }
                else
                {
                    _game.PacketNotifier.NotifyS2C_Neutral_Camp_Empty(this, userId: userId);
                }
            }

        }

        protected override void OnSpawn(int userId, TeamId team, bool doVision = false)
        {
            _game.PacketNotifier.NotifyS2C_CreateMinionCamp(this, userId);
        }

        protected override void OnEnterVision(int userId, TeamId team)
        {
        }

        protected override void OnLeaveVision(int userId, TeamId team)
        {
        }

        public IMonster AddMonster(IMonster monster)
        {
            var aiscript = monster.AIScript.ToString().Remove(0, 10);
            var campMonster = new Monster
            (
                _game, monster.Name, monster.Model, monster.Position,
                monster.Direction, this, monster.Team, 0,
                monster.SpawnAnimation, monster.IsTargetable, monster.IgnoresCollision, aiscript,
                monster.DamageBonus, monster.HealthBonus, monster.InitialLevel
            );
            while(campMonster.Stats.Level < monster.InitialLevel)
            {
                campMonster.Stats.LevelUp();
            }
            Monsters.Add(campMonster);
            ApiEventManager.OnDeath.AddListener(campMonster, campMonster, OnMonsterDeath, true);
            _game.ObjectManager.AddObject(campMonster);

            IsAlive = true;
            foreach(TeamId team in _playerTeams)
            {
                if(_teamSawLastDeath[team])
                {
                    _teamSawLastDeath[team] = false;
                    _isAliveForTeam[team] = true;
                }
            }

            return campMonster;
        }

        public void OnMonsterDeath(IDeathData deathData)
        {
            IMonster monster = deathData.Unit as IMonster;
            Monsters.Remove(monster);
            if (Monsters.Count == 0)
            {
                IsAlive = false;
                foreach(TeamId team in _playerTeams)
                {
                    _teamSawLastDeath[team] = monster.IsVisibleByTeam(team) || IsVisibleByTeam(team);
                    if (_teamSawLastDeath[team])
                    {
                        _isAliveForTeam[team] = false;
                    }
                }
            }
        }
    }
}

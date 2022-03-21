using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;

namespace GameServerLib.GameObjects
{
    public class MonsterCamp : IMonsterCamp
    {
        public byte CampIndex { get; set; }
        public Vector3 Position { get; set; }
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

        private Game _game;
        public MonsterCamp(Game game, Vector3 position, byte groupNumber, TeamId teamSideOfTheMap, string campTypeIcon, float respawnTimer, bool doPlayVO = true, byte revealEvent = 74, float spawnDuration = 0.0f)
        {
            _game = game;
            Position = position;
            CampIndex = groupNumber;
            RevealEvent = revealEvent;
            MinimapIcon = campTypeIcon;
            RespawnTimer = respawnTimer;
            SideTeamId = (byte)teamSideOfTheMap;

            if (teamSideOfTheMap == TeamId.TEAM_NEUTRAL)
            {
                SideTeamId = 0;
            }

            SpawnDuration = spawnDuration;

            game.PacketNotifier.NotifyS2C_CreateMinionCamp(this);
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
            NotifyCampActivation();
            return campMonster;
        }

        public void OnMonsterDeath(IDeathData deathData)
        {
            IMonster monster = deathData.Unit as IMonster;
            Monsters.Remove(monster);
            if (Monsters.Count == 0)
            {
                NotifyCampDeactivation(deathData);
            }
        }

        public void NotifyCampActivation()
        {
            IsAlive = true;
            _game.PacketNotifier.NotifyS2C_ActivateMinionCamp(this);
        }

        public void NotifyCampDeactivation(IDeathData deathData = null)
        {
            IsAlive = false;
            _game.PacketNotifier.NotifyS2C_Neutral_Camp_Empty(this, deathData);
        }
    }
}

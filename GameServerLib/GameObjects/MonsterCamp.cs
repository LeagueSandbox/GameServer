using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServerLib.GameObjects
{
    public class MonsterCamp : IMonsterCamp
    {
        public byte CampIndex { get; set; }
        public Vector3 Position { get; set; }
        public TeamId SideTeamId { get; set; }
        public string MinimapIcon { get; set; }
        public byte RevealEvent { get; set; }
        public float Expire { get; set; }
        public int TimerType { get; set; }
        public float SpawnDuration { get; set; }
        public float DoPlayVO { get; set; }
        //Considering renaming this to just "IsAlive"
        public bool IsAlive { get; set; } = false;
        public float RespawnTimer { get; set; }

        //Double-Check if we want these lists to be public
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
            SideTeamId = teamSideOfTheMap;
            SpawnDuration = spawnDuration;

            game.PacketNotifier.NotifyS2C_CreateMinionCamp(this);
        }
        public void AddMonster(IMonster monster)
        {
            _game.ObjectManager.AddObject(monster);

            Monsters.Add(monster);
            ApiEventManager.OnDeath.AddListener(monster, monster, OnMonsterDeath, true);
        }
        public void OnMonsterDeath(IDeathData deathData)
        {
            IMonster monster = deathData.Unit as IMonster;
            Monsters.Remove(monster);
            if (Monsters.Count == 0)
            {
                IsAlive = false;
                NotifyCampDeactivation(deathData);
            }
        }

        public void NotifyCampActivation()
        {
            _game.PacketNotifier.NotifyS2C_ActivateMinionCamp(this);
        }
        public void NotifyCampDeactivation(IDeathData deathData = null)
        {
            _game.PacketNotifier.NotifyS2C_Neutral_Camp_Empty(this, deathData);
        }
    }
}

using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Fountain
    {
        private const float PERCENT_MAX_HEALTH_HEAL = 0.15f;
        private const float PERCENT_MAX_MANA_HEAL = 0.15f;
        private const float HEAL_FREQUENCY = 1000f;
        private float _x;
        private float _y;
        private float _fountainSize;
        private long _healTickTimer;
        private TeamId _team;

        public Fountain(TeamId team, float x, float y, float size)
        {
            _x = x;
            _y = y;
            _fountainSize = size;
            _healTickTimer = 0;
            _team = team;
        }

        internal void Update(Map map, long diff)
        {
            _healTickTimer += diff;
            if (_healTickTimer < HEAL_FREQUENCY)
                return;

            _healTickTimer = 0;

            var champions = map.GetChampionsInRange(_x, _y, _fountainSize, true);
            foreach (var champion in champions)
            {
                if (champion.Team != _team)
                    continue;

                var hp = champion.GetStats().CurrentHealth;
                var maxHP = champion.GetStats().HealthPoints.Total;
                if (hp + maxHP * PERCENT_MAX_HEALTH_HEAL < maxHP)
                    champion.GetStats().CurrentHealth = hp + maxHP * PERCENT_MAX_HEALTH_HEAL;
                else
                    champion.GetStats().CurrentHealth = maxHP;

                var mp = champion.GetStats().CurrentMana;
                var maxMp = champion.GetStats().ManaPoints.Total;
                if (mp + maxMp * PERCENT_MAX_MANA_HEAL < maxMp)
                    champion.GetStats().CurrentMana = mp + maxMp * PERCENT_MAX_MANA_HEAL;
                else
                    champion.GetStats().CurrentMana = maxMp;
            }
        }
    }
}

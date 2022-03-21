using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;

namespace MapScripts.Map11
{
    public static class MonsterDataTable
    {
        static List<float> AttackDamage = new List<float>
        {
            1.0f,
            1.0f,
            1.0f,
            1.0f,
            1.1f,
            1.2000000000000002f,
            1.3f,
            1.4000000000000001f,
            1.5f,
            1.6f,
            1.7000000000000002f,
            1.9000000000000001f,
            2.1f,
            2.3000000000000003f,
            2.5f,
            2.7f,
            2.9000000000000004f,
            3.1f,
            3.3000000000000003f,
            3.5f
        };

        static List<float> Gold = new List<float>
        {
            1.0f,
            1.0f,
            1.0f,
            1.0f,
            1.0099999904632568f,
            1.0199999809265137f,
            1.0299999713897705f,
            1.0399999618530273f,
            1.0499999523162842f,
            1.059999942779541f,
            1.0700000524520874f,
            1.0800000429153442f,
            1.090000033378601f,
            1.1f,
            1.1200000047683716f,
            1.1399999856948853f,
            1.159999966621399f,
            1.1799999475479126f,
            1.2000000000000002f,
            1.2200000286102295f
        };

        static List<float> Health = new List<float>
        {
            1.0f,
            1.0f,
            1.0f,
            1.0f,
            1.0499999523162842f,
            1.1f,
            1.149999976158142f,
            1.25f,
            1.350000023841858f,
            1.4500000476837158f,
            1.5499999523162842f,
            1.649999976158142f,
            1.75f,
            2.0f,
            2.0f,
            2.0f,
            2.0f,
            2.0f,
            2.0f,
            2.0f
        };

        public static void UpdateStats(IMonster monster)
        {
            var level = monster.Stats.Level;
            //Taking into account URF's max level cap at 30
            if(level > 19)
            {
                level = 19;
            }

            //The Attack damage doesn't get updated on the Monster's HUD, i already double checked and the value is right though.
            monster.Stats.AttackDamage.BaseValue *= AttackDamage[level];
            monster.Stats.GoldGivenOnDeath.BaseValue *= Gold[level];
            monster.Stats.HealthPoints.BaseValue *= Health[level];
            monster.Stats.CurrentHealth = monster.Stats.HealthPoints.Total;
        }
    }
}

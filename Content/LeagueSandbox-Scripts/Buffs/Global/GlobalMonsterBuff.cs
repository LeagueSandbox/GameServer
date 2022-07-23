using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class GlobalMonsterBuff : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Buff thisBuff;
        AttackableUnit owner;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            thisBuff = buff;
            owner = unit;
        }

        public void OnUpdate(float diff)
        {
            //if (owner != null)
            //{
            //    var hpPercent = (owner.Stats.HealthPoints.Total - owner.Stats.CurrentHealth) / owner.Stats.HealthPoints.Total;
            //    if (hpPercent >= 0.995f)
            //    {
            //        // TODO:
            //        // Increase health, physical damage, gold reward, and exp reward every 60 seconds
            //        // Based on camp life time (gameTime - spawnTime)
            //    }
            //}
        }
    }
}

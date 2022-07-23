using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    // NOTE: May or may not be the proper functionality (unknown what this script is actually supposed to do).
    internal class AscHardModeEvent : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
        };

        public StatsModifier StatsModifier { get; private set; }

        Champion champion;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            if(unit is Champion ch)
            {
                champion = ch;
            }
        }

        float timer = 0;
        public void OnUpdate(float diff)
        {
            timer -= diff;
            if(timer <= 0 && champion != null)
            {
                champion.AddExperience(10.0f, false);
                timer = 1000.0f;
            }
        }
    }
}

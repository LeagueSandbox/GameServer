using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    // NOTE: May or may not be the proper functionality (unknown what this script is actually supposed to do).
    internal class AscHardModeEvent : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
        };

        public IStatsModifier StatsModifier { get; private set; }

        IChampion champion;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if(unit is IChampion ch)
            {
                champion = ch;
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
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

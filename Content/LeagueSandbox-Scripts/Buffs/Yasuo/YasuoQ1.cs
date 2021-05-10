using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;

namespace YasuoQ01
{
    internal class YasuoQ01 : IBuffGameScript
    {
        public BuffType BuffType => BuffType.INTERNAL;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => true;

        public IStatsModifier StatsModifier { get; private set; }

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ((IObjAiBase)unit).SetSpell("YasuoQ2W", 0, true);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (((IObjAiBase)unit).Spells[0].SpellName == "YasuoQ2W")
            {
                ((IObjAiBase)unit).SetSpell("YasuoQW", 0, true);
            }
        }

        public void OnUpdate(float diff)
        {
            //nothing!
        }
    }
}

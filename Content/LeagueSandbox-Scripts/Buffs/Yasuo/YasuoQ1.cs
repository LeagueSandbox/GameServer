using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace YasuoQ01
{
    internal class YasuoQ01 : IBuffGameScript
    {
        public BuffType BuffType => BuffType.INTERNAL;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => true;

        public IStatsModifier StatsModifier { get; private set; }

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
            ((IChampion)unit).SetSpell("YasuoQ2W", 0, true);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            if (((IChampion)unit).Spells[0].SpellName == "YasuoQ2W")
            {
                ((IChampion)unit).SetSpell("YasuoQW", 0, true);
            }
        }

        public void OnUpdate(double diff)
        {
            //nothing!
        }
    }
}

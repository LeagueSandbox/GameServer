using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;

namespace YasuoQ02
{
    internal class YasuoQ02 : IBuffGameScript
    {
        public BuffType BuffType => BuffType.INTERNAL;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => true;

        public IStatsModifier StatsModifier { get; private set; }

        private IParticle p1;
        private IParticle p2;
        private IParticle p3;
        private IParticle p4;

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
            ((IChampion)unit).SetSpell("YasuoQ3W", 0, true);
            p1 = AddParticleTarget((IChampion)unit, "Yasuo_Base_Q3_Indicator_Ring.troy", unit);
            p2 = AddParticleTarget((IChampion)unit, "Yasuo_Base_Q3_Indicator_Ring_alt.troy", unit);
            p3 = AddParticleTarget((IChampion)unit, "Yasuo_Base_Q_wind_ready_buff.troy", unit);
            p4 = AddParticleTarget((IChampion)unit, "Yasuo_Base_Q_strike_build_up_test.troy", unit);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            if (((IChampion)unit).Spells[0].SpellName == "YasuoQ3W")
            {
                ((IChampion)unit).SetSpell("YasuoQW", 0, true);
            }
            RemoveParticle(p1);
            RemoveParticle(p2);
            RemoveParticle(p3);
            RemoveParticle(p4);
        }

        public void OnUpdate(double diff)
        {
            //empty!
        }
    }
}

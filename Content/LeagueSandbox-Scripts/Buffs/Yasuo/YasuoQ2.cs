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
        private IBuff _visualBuff;
        private Particle p1;
        private Particle p2;
        private Particle p3;
        private Particle p4;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            ((IChampion)unit).SetSpell("YasuoQ3W", 0, true);
            p1 = AddParticleTarget((IChampion)unit, "Yasuo_Base_Q3_Indicator_Ring.troy", unit);
            p2 = AddParticleTarget((IChampion)unit, "Yasuo_Base_Q3_Indicator_Ring_alt.troy", unit);
            p3 = AddParticleTarget((IChampion)unit, "Yasuo_Base_Q_wind_ready_buff.troy", unit);
            p4 = AddParticleTarget((IChampion)unit, "Yasuo_Base_Q_strike_build_up_test.troy", unit);
            _visualBuff = AddBuffHudVisual("YasuoQ3W", 6f, 1, BuffType.COMBAT_ENCHANCER, unit);
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
            RemoveBuffHudVisual(_visualBuff);
        }

        public void OnUpdate(double diff)
        {
            //empty!
        }
    }
}

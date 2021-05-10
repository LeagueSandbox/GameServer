using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;

namespace GarenE
{
    internal class GarenE : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_ENCHANCER;

        public BuffAddType BuffAddType => BuffAddType.RENEW_EXISTING;

        public bool IsHidden => true;

        public int MaxStacks => 1;

        public IStatsModifier StatsModifier { get; private set; }

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            // TODO: allow garen move through units
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            // TODO: disallow garen move through units
        }

        public void OnUpdate(float diff)
        {
            
        }
    }
}

using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;

namespace Stun
{
    internal class Stun : IBuffGameScript
    {
        public BuffType BuffType => BuffType.STUN;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => true;

        public IStatsModifier StatsModifier { get; private set; }

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        public void OnDeactivate(IAttackableUnit unit)
        {
        }

        public void OnUpdate(double diff)
        {

        }
    }
}
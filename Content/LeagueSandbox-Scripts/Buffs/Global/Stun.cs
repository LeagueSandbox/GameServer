using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Stun
{
    internal class Stun : IBuffGameScript
    {
        public BuffType BuffType => BuffType.STUN;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => true;

        public IStatsModifier StatsModifier { get; private set; }

        IParticle stun;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            stun = AddParticleTarget(ownerSpell.CastInfo.Owner, "Global_Stun.troy", unit, buff.Duration, "head");
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(stun);
        }

        public void OnUpdate(float diff)
        {

        }
    }
}
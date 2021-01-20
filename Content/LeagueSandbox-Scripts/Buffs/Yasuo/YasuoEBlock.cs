using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace YasuoEBlock
{
    internal class YasuoEBlock : IBuffGameScript
    {
        public BuffType BuffType => BuffType.INTERNAL;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => true;

        public IStatsModifier StatsModifier { get; private set; }

        private readonly IChampion owner = Spells.YasuoDashWrapper._owner;
        private IBuff _visualBuff;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var time = 11f - ownerSpell.Level * 1f;
            AddParticleTarget(ownerSpell.Owner, "Yasuo_base_E_timer1.troy", unit);
        }

        public void OnDeactivate(IAttackableUnit unit)
        {
        }

        public void OnUpdate(double diff)
        {
            //empty!
        }
    }
}

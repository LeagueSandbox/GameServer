using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace YasuoEBlock
{
    internal class YasuoEBlock : IBuffGameScript
    {
        public BuffType BuffType { get; } = BuffType.INTERNAL;
        public BuffAddType BuffAddType { get; } = BuffAddType.REPLACE_EXISTING;
        public int MaxStacks { get; } = 1;
        public bool IsHidden { get; } = true;
        public bool IsUnique { get; } = true;

        public IStatsModifier StatsModifier { get; private set; }

        private readonly IChampion owner = Spells.YasuoDashWrapper._owner;
        private IBuff _visualBuff;

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
            var time = 11f - ownerSpell.Level * 1f;
            AddParticleTarget(owner, "Yasuo_base_E_timer1.troy", unit);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
        }

        public void OnUpdate(double diff)
        {
            //empty!
        }
    }
}

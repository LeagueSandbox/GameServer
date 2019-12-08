using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;

namespace YasuoEBlock
{
    internal class YasuoEBlock : IBuffGameScript
    {
        private StatsModifier _statMod;
        private IBuff _visualBuff;
        private IChampion owner = Spells.YasuoDashWrapper._owner;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            var time = 11f - ownerSpell.Level * 1f;
            AddBuffHudVisual("YasuoDashScalar", time, 1, BuffType.COMBAT_DEHANCER, unit, time);
            AddParticleTarget(owner, "Yasuo_base_E_timer1.troy", unit);
        }

        public void OnDeactivate(IObjAiBase unit)
        {

        }

        public void OnUpdate(double diff)
        {

        }
    }
}

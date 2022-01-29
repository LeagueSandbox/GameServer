using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class SionPassiveDelay : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_DEHANCER
        };

        public IStatsModifier StatsModifier { get; private set; }

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            buff.SetStatusEffect(StatusFlags.Targetable, false);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var champion = unit as IChampion;
            champion.Respawn();//Implement a custom spawn position function later
            SetStatus(unit, StatusFlags.Targetable, true);
            AddBuff("SionPassive", 60f, 1, ownerSpell, unit, champion);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

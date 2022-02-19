using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    class Recall : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_DEHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; }

        private IChampion owner;
        private IBuff sourceBuff;
        bool willRemove;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            owner = unit as IChampion;
            sourceBuff = buff;

            ApiEventManager.OnTakeDamage.AddListener(this, unit, OnTakeDamage, true);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ApiEventManager.OnTakeDamage.RemoveListener(this, ownerSpell.CastInfo.Owner);
        }

        private void OnTakeDamage(IDamageData damageData)
        {
            if (damageData.DamageSource != DamageSource.DAMAGE_SOURCE_PERIODIC)
            {
                willRemove = true;
            }
        }

        public void OnUpdate(float diff)
        {
            if (willRemove)
            {
                StopChanneling(owner, ChannelingStopCondition.Cancel, ChannelingStopSource.LostTarget);

                RemoveBuff(sourceBuff);
            }
        }
    }
}

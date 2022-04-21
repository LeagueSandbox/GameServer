using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    public class ZedW2 : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        ZedWHandler Handler;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (unit is IObjAiBase owner)
            {
                Handler = (owner.GetBuffWithName("ZedWHandler").BuffScript as ZedWHandler);
                var w2Spell = SetSpell(owner, "ZedW2", SpellSlotType.SpellSlots, 1);
                ApiEventManager.OnSpellCast.AddListener(this, w2Spell, Handler.ShadowSwap);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (!Handler.QueueSwap)
            {
                unit.RemoveBuffsWithName("ZedWHandler");
            }

            if (unit is IObjAiBase ai)
            {
                SetSpell(ai, "ZedShadowDash", SpellSlotType.SpellSlots, 1);
            }
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

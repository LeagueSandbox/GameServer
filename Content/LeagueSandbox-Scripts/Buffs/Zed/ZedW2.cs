using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    public class ZedW2 : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        ZedWHandler Handler;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            if (unit is ObjAIBase owner)
            {
                Handler = (owner.GetBuffWithName("ZedWHandler").BuffScript as ZedWHandler);
                var w2Spell = SetSpell(owner, "ZedW2", SpellSlotType.SpellSlots, 1);
                ApiEventManager.OnSpellCast.AddListener(this, w2Spell, Handler.ShadowSwap);
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            if (!Handler.QueueSwap)
            {
                unit.RemoveBuffsWithName("ZedWHandler");
            }

            if (unit is ObjAIBase ai)
            {
                SetSpell(ai, "ZedShadowDash", SpellSlotType.SpellSlots, 1);
            }
        }
    }
}

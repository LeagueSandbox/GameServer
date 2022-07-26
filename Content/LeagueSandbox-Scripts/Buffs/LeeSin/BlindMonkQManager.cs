using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class BlindMonkQManager : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; }

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            var owner = unit as ObjAIBase;

            if (owner != null)
            {
                var qTwoSpell = SetSpell(owner, "BlindMonkQTwo", SpellSlotType.SpellSlots, 0);
                // TODO: Instead of doing this, simply make an API function for SetSpellSlotCooldown
                qTwoSpell.SetCooldown(0, true);

                var qOneBuff = buff.SourceUnit.GetBuffWithName("BlindMonkQOne");
                var qOneChaosBuff = buff.SourceUnit.GetBuffWithName("BlindMonkQOneChaos");

                if (qOneBuff == null && qOneChaosBuff == null)
                {
                    RemoveBuff(buff);
                }
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            var owner = unit as ObjAIBase;

            if (owner != null)
            {
                float[] cdByLevel = { 7f, 6f, 5f, 4f, 3f };
                var newCooldown = (MathF.Max(buff.TimeElapsed - 3.0f, 0f) + cdByLevel[ownerSpell.CastInfo.SpellLevel - 1]) * (1 + owner.Stats.CooldownReduction.Total);

                var qTwoSpell = SetSpell(owner, "BlindMonkQOne", SpellSlotType.SpellSlots, 0);
                // TODO: Instead of doing this, simply make an API function for SetSpellSlotCooldown
                qTwoSpell.SetCooldown(newCooldown, true);
            }
        }
    }
}
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System;

namespace Buffs
{
    internal class BlindMonkQManager : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; }

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var owner = unit as IObjAiBase;

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

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var owner = unit as IObjAiBase;

            if (owner != null)
            {
                float[] cdByLevel = { 7f, 6f, 5f, 4f, 3f };
                var newCooldown = (MathF.Max(buff.TimeElapsed - 3.0f, 0f) + cdByLevel[ownerSpell.CastInfo.SpellLevel - 1]) * owner.Stats.CooldownReduction.Total;

                var qTwoSpell = SetSpell(owner, "BlindMonkQOne", SpellSlotType.SpellSlots, 0);
                // TODO: Instead of doing this, simply make an API function for SetSpellSlotCooldown
                qTwoSpell.SetCooldown(newCooldown, true);
            }
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
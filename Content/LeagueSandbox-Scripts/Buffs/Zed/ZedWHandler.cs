using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    class ZedWHandler : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Spell ThisSpell;
        Buff ThisBuff;
        Minion Shadow;
        Buff ShadowBuff;
        public bool QueueSwap;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            ThisSpell = ownerSpell;
            ThisBuff = buff;

            ClearShadows();
        }

        public Minion ShadowSpawn()
        {
            var owner = ThisSpell.CastInfo.Owner;

            var spellPos = new Vector2(ThisSpell.CastInfo.TargetPositionEnd.X, ThisSpell.CastInfo.TargetPositionEnd.Z);
            Shadow = AddMinion(owner, "ZedShadow", "ZedShadow", spellPos, owner.Team, owner.SkinID, true, false);

            var goingTo = new Vector2(ThisSpell.CastInfo.TargetPositionEnd.X, ThisSpell.CastInfo.TargetPositionEnd.Z) - owner.Position;
            var dirTemp = Vector2.Normalize(goingTo);
            Shadow.FaceDirection(new Vector3(dirTemp.X, 0, dirTemp.Y));

            AddBuff("ZedWShadowBuff", ThisBuff.Duration, 1, ThisSpell, Shadow, owner);

            return Shadow;
        }

        /// <summary>
        /// Perform a cast of the given spell using the shadow (only applies to Q and E).
        /// </summary>
        /// <param name="spell">Spell which triggered this shadow cast.</param>
        /// TODO: Test this with Q and E.
        public void ShadowCast(Spell spell)
        {
            var slot = spell.CastInfo.SpellSlot;
            if (slot != 0 || slot != 2 && Shadow != null)
            {
                return;
            }

            FaceDirection(new Vector2(spell.CastInfo.TargetPositionEnd.X, spell.CastInfo.TargetPositionEnd.Z), Shadow, true);
            SpellCast(spell.CastInfo.Owner, slot, SpellSlotType.SpellSlots, true, spell.CastInfo.Targets[0].Unit, Shadow.Position);

            switch (slot)
            {
                case 0:
                {
                    PlayAnimation(Shadow, "Spell1");
                    return;
                }
                case 2:
                {
                    PlayAnimation(Shadow, "Spell3");
                    return;
                }
            }
        }

        public void ShadowSwap(Spell spell)
        {
            var owner = spell.CastInfo.Owner;

            if (Shadow != null && !Shadow.IsDead)
            {
                if (QueueSwap)
                {
                    QueueSwap = false;
                }
                var ownerPos = owner.Position;
                TeleportTo(owner, Shadow.Position.X, Shadow.Position.Y);
                AddParticleTarget(owner, owner, "zed_base_cloneswap", owner);
                TeleportTo(Shadow, ownerPos.X, ownerPos.Y);
                AddParticleTarget(owner, Shadow, "zed_base_cloneswap", Shadow);

                owner.RemoveBuffsWithName("ZedW2");
                owner.RemoveBuff(ThisBuff);
            }
            else if (Shadow == null)
            {
                QueueSwap = true;
                owner.RemoveBuffsWithName("ZedW2");
            }
        }

        public void ClearShadows()
        {
            if (ShadowBuff != null)
            {
                LogInfo("Previous shadow found.");
                if (!ShadowBuff.Elapsed())
                {
                    ShadowBuff.DeactivateBuff();
                }
            }
        }

        public void OnUpdate(float diff)
        {
            if (Shadow != null && QueueSwap)
            {
                ShadowSwap(ThisSpell);
            }
        }
    }
}

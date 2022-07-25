using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace Buffs
{
    internal class AscWarpTarget : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; }

        Particle p1;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            unit.IconInfo.ChangeBorder("Teleport", "ascwarptarget");
            unit.IconInfo.ChangeIcon("NoIcon");
            p1 = AddParticleTarget(buff.SourceUnit, null, "global_asc_teleport_target", unit, -1);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveParticle(p1);
            unit.IconInfo.ResetBorder();
            TeleportTo(buff.SourceUnit, unit.Position.X, unit.Position.Y);
            if (buff.SourceUnit is Champion ch)
            {
                TeleportCamera(ch, new Vector3(unit.Position.X, unit.GetHeight(), unit.Position.Y));
            }
            unit.Die(CreateDeathData(false, 3, unit, unit, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_RAW, 0.0f));
            buff.SourceUnit.Spells[6 + (byte)SpellSlotType.InventorySlots].SetCooldown(float.MaxValue, true);
        }
    }
}
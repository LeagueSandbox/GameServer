using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class SRU_Turret_Laser_Audio : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.AURA,
            BuffAddType = BuffAddType.STACKS_AND_RENEWS,
            MaxStacks = 1,
            IsHidden = true
        };

        public StatsModifier StatsModifier { get; private set; }

        Particle p1;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
                p1 = AddParticleTarget(unit, unit, "SRU_Inhibitor_Tower_Chaos_Beam_Lvl1_Audio", unit, 0.5f, flags: (FXFlags)32);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            p1.SetToRemove();
        }
    }
}

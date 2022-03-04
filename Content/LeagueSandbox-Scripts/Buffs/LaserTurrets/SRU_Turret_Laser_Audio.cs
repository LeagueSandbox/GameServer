using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class SRU_Turret_Laser_Audio : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.AURA,
            BuffAddType = BuffAddType.STACKS_AND_RENEWS,
            MaxStacks = 1,
            IsHidden = true
        };

        public IStatsModifier StatsModifier { get; private set; }

        IParticle p1;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
                p1 = AddParticleTarget(unit, unit, "SRU_Inhibitor_Tower_Chaos_Beam_Lvl1_Audio", unit, 0.5f, flags: (FXFlags)32);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            p1.SetToRemove();
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

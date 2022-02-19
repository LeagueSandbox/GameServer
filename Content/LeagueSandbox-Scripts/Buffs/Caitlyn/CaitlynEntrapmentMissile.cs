using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Collections.Generic;

namespace Buffs
{
    internal class CaitlynEntrapmentMissile : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.SLOW,
            BuffAddType = BuffAddType.STACKS_AND_OVERLAPS,
            MaxStacks = 100,
            IsHidden = true
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle slowParticle;

        public void OnUpdate(float diff)
        {
        }

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            slowParticle = AddParticleTarget(unit, unit, "caitlyn_entrapment_slow", unit, buff.Duration);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(slowParticle);
        }
    }
}

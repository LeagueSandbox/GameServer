using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class RegenerationPotion : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.HEAL,
            BuffAddType = BuffAddType.STACKS_AND_CONTINUE,
            MaxStacks = 5
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle potion;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var caster = ownerSpell.CastInfo.Owner;
            StatsModifier.HealthRegeneration.FlatBonus = 10f;
            unit.AddStatModifier(StatsModifier);
            potion = AddParticleTarget(caster, unit, "GLOBAL_Item_HealthPotion", unit, buff.Duration, bone: "Buffbone_Glb_Ground_Loc");
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            potion.SetToRemove();
        }

        public void OnUpdate(float diff)
        {

        }
    }
}

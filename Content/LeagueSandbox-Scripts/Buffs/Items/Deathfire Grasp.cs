using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Deathfire_Grasp
{
    internal class Deathfire_Grasp : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_DEHANCER;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle target;
        IParticle debuff;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            target = AddParticleTarget(ownerSpell.CastInfo.Owner, "deathFireGrasp_tar.troy", target);
            debuff = AddParticleTarget(ownerSpell.CastInfo.Owner, "obj_DeathfireGrasp_debuff.troy", target);

            // TODO: Implement damage amp. stat modifier
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(target);
            RemoveParticle(debuff);
        }

        public void OnUpdate(float diff)
        {

        }
    }
}

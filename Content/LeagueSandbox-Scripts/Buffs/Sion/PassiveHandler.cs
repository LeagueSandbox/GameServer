using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;


namespace Buffs
{
    internal class SionPassiveDelay : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_DEHANCER;
        public BuffAddType BuffAddType => BuffAddType.RENEW_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; }

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            unit.SetIsTargetableToTeam(TeamId.TEAM_BLUE, false);
            unit.SetIsTargetableToTeam(TeamId.TEAM_PURPLE, false);
            unit.SetIsTargetableToTeam(TeamId.TEAM_PURPLE, false);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var champion = unit as IChampion;
            champion.Respawn();//Implement a custom spawn position function later
            unit.SetIsTargetableToTeam(TeamId.TEAM_BLUE, true);
            unit.SetIsTargetableToTeam(TeamId.TEAM_PURPLE, true);
            unit.SetIsTargetableToTeam(TeamId.TEAM_PURPLE, true);
            AddBuff("SionPassive", 60f, 1, ownerSpell, unit, champion);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

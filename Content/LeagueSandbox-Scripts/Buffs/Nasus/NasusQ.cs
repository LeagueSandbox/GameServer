using System;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace NasusQ
{
    class NasusQ : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_ENCHANCER;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle pbuff;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            pbuff = AddParticleTarget(unit, "Nasus_Base_Q_Buf.troy", unit, 1, "BUFFBONE_CSTM_WEAPON_1");

            StatsModifier.Range.FlatBonus = Math.Abs(unit.Stats.Range.Total - 150f);

            unit.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IAttackableUnit unit)
        {
            RemoveParticle(pbuff);
        }

        public void OnUpdate(float diff)
        {

        }
    }
}

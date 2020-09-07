using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace GarenE
{
    internal class GarenE : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_ENCHANCER;

        public BuffAddType BuffAddType => BuffAddType.RENEW_EXISTING;

        public bool IsHidden => true;

        public int MaxStacks => 1;

        public IStatsModifier StatsModifier => null;

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
            // TODO: allow garen move through units
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            // TODO: disallow garen move through units
        }

        public void OnUpdate(double diff)
        {
            
        }
    }
}

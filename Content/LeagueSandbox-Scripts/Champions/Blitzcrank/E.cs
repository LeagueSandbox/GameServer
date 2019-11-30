using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Spells
{
    public class PowerFist : ISpellScript
    {
        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            AddBuffHudVisual("Powerfist", 5, 1, BuffType.COMBAT_ENCHANCER, owner, 5);
            //owner.AddBuffGameScript("Powerfist", "Powerfist", spell, 8.0f);
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {

        }

        public void CooldownStarted(IChampion owner, ISpell spell)        {            //Executed once spell cooldown started        }

        public void CooldownEnded(IChampion owner, ISpell spell)
        {
            //Executed when cooldown is over
        }
    }
}

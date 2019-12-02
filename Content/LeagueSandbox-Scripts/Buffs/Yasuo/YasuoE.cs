using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;

namespace YasuoE
{
    internal class YasuoE : IBuffGameScript
    {
        private StatsModifier _statMod;
        private IBuff _visualBuff;
        private IAttackableUnit target = Spells.YasuoDashWrapper._target;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            var time = 0.6f - ownerSpell.Level * 0.1f;
            var damage = 50f + ownerSpell.Level * 20f + unit.Stats.AbilityPower.Total * 0.6f;
            AddParticleTarget((IChampion)unit, "Yasuo_Base_E_Dash.troy", unit);
            AddParticleTarget((IChampion)unit, "Yasuo_Base_E_dash_hit.troy", target);
            AddBuffHudVisual("YasuoDash", time, 1, BuffType.COMBAT_DEHANCER, unit, time);
            var to = Vector2.Normalize(target.GetPosition() - unit.GetPosition());
            //var trueCoords = unit.GetPosition() + to * 100f;
            DashToLocation(unit, target.X + to.X * 235f, target.Y + to.Y * 235f, 750f + unit.Stats.MoveSpeed.Total * 0.6f, false, "SPELL3");
            target.TakeDamage(unit, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            CancelDash(unit);
        }

        public void OnUpdate(double diff)
        {

        }
    }
}

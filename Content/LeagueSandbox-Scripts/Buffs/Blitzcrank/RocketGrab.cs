using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Buffs
{
    internal class RocketGrab : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Particle grab;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            grab = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "FistReturn_mis", ownerSpell.CastInfo.Owner, buff.Duration, 1, "head", "R_hand");
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveParticle(grab);
        }
    }
}


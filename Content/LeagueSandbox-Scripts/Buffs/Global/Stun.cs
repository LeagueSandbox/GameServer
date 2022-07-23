using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class Stun : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.STUN,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; }

        Particle stun;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            buff.SetStatusEffect(StatusFlags.Stunned, true);
            stun = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "LOC_Stun", unit, buff.Duration, bone: "head");
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveParticle(stun);
        }
    }
}
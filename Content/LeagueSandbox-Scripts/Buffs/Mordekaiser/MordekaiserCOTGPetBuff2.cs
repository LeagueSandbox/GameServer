using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiEventManager;

namespace Buffs
{
    internal class MordekaiserCOTGPetBuff2 : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.AURA,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Buff Buff;
        Particle p;
        Particle p2;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            Buff = buff;

            AddBuff("MordekaiserCOTGPetBuff", buff.Duration, 1, ownerSpell, unit, buff.SourceUnit);
            buff.SourceUnit.SetSpell("MordekaiserCotGGuide", 3, true);
            AddBuff("MordekaiserCOTGSelf", buff.Duration, 1, ownerSpell, buff.SourceUnit, buff.SourceUnit);

            AddParticleTarget(buff.SourceUnit, unit, "mordekaiser_cotg_ring", unit, buff.Duration);
            AddParticleTarget(buff.SourceUnit, unit, "mordekeiser_cotg_skin", unit, buff.Duration);

            OnDeath.AddListener(this, unit, OnGhostDeath, true);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            //In theory the killer should be null, but that causes the ghost to not die(?)
            unit.Die(CreateDeathData(false, 0, unit, unit, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_INTERNALRAW, 0.0f));
            RemoveParticle(p);
            RemoveParticle(p2);

            RemoveBuff(buff.SourceUnit, "MordekaiserCOTGSelf");
            Spell spell = buff.SourceUnit.SetSpell("MordekaiserChildrenOfTheGrave", 3, true);
            //Check if this is done on-script or should be handled automatically
            spell.SetCooldown(spell.GetCooldown() - buff.TimeElapsed);
        }

        public void OnGhostDeath(DeathData data)
        {
            Buff.DeactivateBuff();
        }
    }
}

using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    class AatroxR : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        string pmodelname;
        Particle pmodel;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            if (unit is Champion c)
            {
                // TODO: Implement Animation Overrides for spells like these
                if (c.SkinID == 0)
                {
                    pmodelname = "Aatrox_Base_RModel";
                }
                else if (c.SkinID == 1)
                {
                    pmodelname = "Aatrox_Skin01_RModel";
                }
                else if (c.SkinID == 2)
                {
                    pmodelname = "Aatrox_Skin02_RModel";
                }
                pmodel = AddParticleTarget(c, c, pmodelname, c);
                pmodel.SetToRemove();

                StatsModifier.AttackSpeed.PercentBonus = (0.4f + (0.1f * (ownerSpell.CastInfo.SpellLevel - 1))) * buff.StackCount; // StackCount included here as an example
                StatsModifier.Range.FlatBonus = 175f * buff.StackCount;

                unit.AddStatModifier(StatsModifier);
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveParticle(pmodel);
        }

        public void OnUpdate(float diff)
        {

        }
    }
}

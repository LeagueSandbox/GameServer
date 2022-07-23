using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class AatroxR : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        string pcastname;
        string phitname;
        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            if (owner is Champion c)
            {
                if (c.SkinID == 0)
                {
                    pcastname = "Aatrox_Base_R_Activate";
                    phitname = "Aatrox_Base_R_active_hit_tar";
                }
                else if (c.SkinID == 1)
                {
                    pcastname = "Aatrox_Skin01_R_Activate";
                    phitname = "Aatrox_Skin01_R_active_hit_tar";
                }
                else if (c.SkinID == 2)
                {
                    pcastname = "Aatrox_Skin02_R_Activate";
                    phitname = "Aatrox_Skin02_R_active_hit_tar";
                }
            }
        }

        public void OnSpellCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            AddParticleTarget(owner, owner, pcastname, owner);
        }

        public void OnSpellPostCast(Spell spell)
        {
            if (spell.CastInfo.Owner is Champion c)
            {
                var damage = 200 + (100 * (spell.CastInfo.SpellLevel - 1)) + (c.Stats.AbilityPower.Total);

                var units = GetUnitsInRange(c.Position, 550f, true);
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i].Team != c.Team && !(units[i] is ObjBuilding || units[i] is BaseTurret))
                    {
                        units[i].TakeDamage(c, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                        AddParticleTarget(c, units[i], phitname, units[i]);
                    }
                }

                AddBuff("AatroxR", 12f, 1, spell, spell.CastInfo.Owner, spell.CastInfo.Owner);
            }
        }
    }
}

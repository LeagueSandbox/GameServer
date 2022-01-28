using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;

namespace Spells
{
    public class AatroxR : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        string pcastname;
        string phitname;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            if (owner is IChampion c)
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

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            AddParticleTarget(owner, owner, pcastname, owner);
        }

        public void OnSpellPostCast(ISpell spell)
        {
            if (spell.CastInfo.Owner is IChampion c)
            {
                var damage = 200 + (100 * (spell.CastInfo.SpellLevel - 1)) + (c.Stats.AbilityPower.Total);

                var units = GetUnitsInRange(c.Position, 550f, true);
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i].Team != c.Team && !(units[i] is IObjBuilding || units[i] is IBaseTurret))
                    {
                        units[i].TakeDamage(c, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                        AddParticleTarget(c, units[i], phitname, units[i]);
                    }
                }

                AddBuff("AatroxR", 12f, 1, spell, spell.CastInfo.Owner, spell.CastInfo.Owner);
            }
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using LeagueSandbox.GameServer.GameObjects.Spells;

namespace Spells
{
    public class AatroxR : IGameScript
    {
        string pcastname;
        string phitname;
        public void OnActivate(IObjAiBase owner)
        {
            if (owner is IChampion c)
            {
                if (c.Skin == 0)
                {
                    pcastname = "Aatrox_Base_R_Activate.troy";
                    phitname = "Aatrox_Base_R_active_hit_tar.troy";
                }
                else if (c.Skin == 1)
                {
                    pcastname = "Aatrox_Skin01_R_Activate.troy";
                    phitname = "Aatrox_Skin01_R_active_hit_tar.troy";
                }
                else if (c.Skin == 2)
                {
                    pcastname = "Aatrox_Skin02_R_Activate.troy";
                    phitname = "Aatrox_Skin02_R_active_hit_tar.troy";
                }
            }
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            AddParticleTarget(owner, pcastname, owner);
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            if (owner is IChampion c)
            {
                var damage = 200 + (100 * (spell.Level - 1)) + (c.Stats.AbilityPower.Total);

                var units = GetUnitsInRange(c, 550f, true);
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i].Team != c.Team && !(units[i] is IObjBuilding || units[i] is IBaseTurret))
                    {
                        units[i].TakeDamage(c, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                        AddParticleTarget(c, phitname, units[i]);
                        //spell.AddProjectileTarget("AatroxRHeal", units[i].X, units[i].Y, owner, true);
                    }
                }

                AddBuff("AatroxR", 12f, 1, spell, owner, owner);
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            //projectile.SetToRemove();
        }

        public void OnUpdate(double diff)
        {
        }
    }
}

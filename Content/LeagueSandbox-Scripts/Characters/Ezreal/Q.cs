using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Spells
{
    public class EzrealMysticShot : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true
        };

        private IObjAiBase _owner;
        private ISpell _spell;
        private float _bonusAd = 0;

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _owner = owner;
            _spell = spell;
            ApiEventManager.OnUpdateStats.AddListener(this, owner, OnStatsUpdate, false);
        }

        public void OnSpellCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            AddParticleTarget(owner, owner, "ezreal_bow", owner, bone: "L_HAND");
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner as IChampion;
            var ownerSkinID = owner.SkinID;
            var targetPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var ownerPos = owner.Position;
            var distance = Vector2.Distance(ownerPos, targetPos);
            FaceDirection(targetPos, owner);

            if (distance > 1200.0)
            {
                targetPos = GetPointFromUnit(owner, 1150.0f);
            }

            if (ownerSkinID == 5)
            {
                SpellCast(owner, 3, SpellSlotType.ExtraSlots, targetPos, targetPos, false, Vector2.Zero);
            }
            else
            {
                SpellCast(owner, 0, SpellSlotType.ExtraSlots, targetPos, targetPos, false, Vector2.Zero);
            }
        }

        private void OnStatsUpdate(IAttackableUnit _unit, float diff)
        {
            float bonusAd = _owner.Stats.AttackDamage.Total * _spell.SpellData.AttackDamageCoefficient;
            if(_bonusAd != bonusAd)
            {
                _bonusAd = bonusAd;
                SetSpellToolTipVar(_owner, 2, bonusAd, SpellbookType.SPELLBOOK_CHAMPION, 0, SpellSlotType.SpellSlots);
            }
        }
    }

    public class EzrealMysticShotMissile : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Circle
            },
            IsDamagingSpell = true
            // TODO
        };

        //Vector2 direction;

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            var owner = spell.CastInfo.Owner;
            var ad = owner.Stats.AttackDamage.Total * spell.SpellData.AttackDamageCoefficient;
            var ap = owner.Stats.AbilityPower.Total * spell.SpellData.MagicDamageCoefficient;
            var damage = 15 + spell.CastInfo.SpellLevel * 20 + ad + ap;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);

            for (byte i = 0; i < 4; i++)
            {
                owner.Spells[i].LowerCooldown(1);
            }

            AddParticleTarget(owner, target, "Ezreal_mysticshot_tar", target);
            missile.SetToRemove();

            // SpellBuffAdd EzrealRisingSpellForce
        }
    }
}

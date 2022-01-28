using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects.Spell.Sector;

namespace Spells
{
    public class LuxMaliceCannon : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
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
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);

            SpellCast(owner, 2, SpellSlotType.ExtraSlots, spellPos, spellPos, false, Vector2.Zero);
        }

        public void OnSpellPostCast(ISpell spell)
        {
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

    public class LuxMaliceCannonMis : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
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
            SetStatus(owner, StatusFlags.ForceRenderParticles, true);
            var startPoint = GetPointFromUnit(owner, 145f);
            var endPoint = GetPointFromUnit(owner, 3400f);
            var tempMinion = AddMinion(owner, "TestCubeRender", "TestCubeRender", startPoint, owner.Team, ignoreCollision: true, targetable: false);
            AddBuff("ExpirationTimer", 2.0f, 1, spell, tempMinion, owner);

            // TODO: Vision

            AddParticle(owner, tempMinion, "luxmalicecannon_beam", endPoint, bone: "top", lifetime: 2.0f);
            AddParticle(owner, tempMinion, "luxmalicecannon_cas", endPoint, bone: "top", lifetime: 2.0f);
            AddParticle(owner, null, "luxmalicecannon_beammiddle", GetPointFromUnit(owner, 1650f));
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            var endPoint = GetPointFromUnit(owner, 3400f);

            spell.CreateSpellSector(new SectorParameters
            {
                BindObject = owner,
                Length = 3400f,
                Width = 100f,
                PolygonVertices = new Vector2[]
                {
                    // Basic square, however the values will be scaled by Length/Width, which will make it a rectangle
                    new Vector2(-1, 0),
                    new Vector2(-1, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0)
                },
                SingleTick = true,
                Type = SectorType.Polygon
            });
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            var owner = spell.CastInfo.Owner;

            var damage = 300f + (100f * spell.CastInfo.SpellLevel - 1) + (owner.Stats.AbilityPower.Total * 0.75f);
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL,
                false);
            AddParticleTarget(owner, target, "luxmalicecannon_tar", target, 1.0f);
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

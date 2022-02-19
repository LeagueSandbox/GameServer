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
    public class LucianQ : ISpellScript
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
            var endPoint = GetPointFromUnit(owner, 1100f);
            //AddParticleTarget(owner, null, "Lucian_Q_cas.troy", owner, lifetime: 1.0f);
            AddParticle(owner, owner, "Lucian_Q_laser_red", endPoint, bone: "C_BUFFBONE_GLB_CENTER_LOC", lifetime: 1.0f);
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            var endPoint = GetPointFromUnit(owner, 1100f);

            spell.CreateSpellSector(new SectorParameters
            {
                BindObject = owner,
                Length = 1100f,
                Width = spell.SpellData.LineWidth,
                PolygonVertices = new Vector2[]
                {
                    // Basic square, however the values will be scaled by HalfLength/Width, which will make it a rectangle
                    new Vector2(-1, 0),
                    new Vector2(-1, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0)
                },
                SingleTick = true,
                Type = SectorType.Polygon
            });

            AddParticle(owner, owner, "Lucian_Q_laser", endPoint, bone: "C_BUFFBONE_GLB_CENTER_LOC", lifetime: 1.0f);
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            var owner = spell.CastInfo.Owner;

            var damage = owner.Stats.AttackDamage.Total * (0.45f + spell.CastInfo.SpellLevel * 0.15f) + (50 + spell.CastInfo.SpellLevel * 30);
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELL,
                false);
            AddParticleTarget(owner, target, "Lucian_Q_tar", target, 1.0f);
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

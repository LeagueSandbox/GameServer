using System.Numerics;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;

namespace Spells
{
    public class LucianQ : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnSpellCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            var castPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            FaceDirection(castPos, owner, true);
            var endPoint = GetPointFromUnit(owner, 1100f);
            //AddParticleTarget(owner, null, "Lucian_Q_cas.troy", owner, lifetime: 1.0f);
            AddParticle(owner, owner, "Lucian_Q_laser_red", endPoint, bone: "C_BUFFBONE_GLB_CENTER_LOC", lifetime: 1.0f);
        }

        public void OnSpellPostCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            var castPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var endPoint = GetPointFromUnit(owner, 1100f);

            FaceDirection(castPos, owner, true);

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

        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            var owner = spell.CastInfo.Owner;

            var damage = owner.Stats.AttackDamage.Total * (0.45f + spell.CastInfo.SpellLevel * 0.15f) + (50 + spell.CastInfo.SpellLevel * 30);
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELL,
                false);
            AddParticleTarget(owner, target, "Lucian_Q_tar", target, 1.0f);
        }
    }
}

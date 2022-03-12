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
    public class BlindMonkQOne : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            NotSingleTargetSpell = true,
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Circle
            }
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
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var current = new Vector2(spell.CastInfo.SpellCastLaunchPosition.X, spell.CastInfo.SpellCastLaunchPosition.Z);
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var to = Vector2.Normalize(spellPos - current);
            var range = to * spell.SpellData.CastRangeDisplayOverride;
            var trueCoords = current + range;
            //spell.AddProjectile("BlindMonkQOne", new Vector2(spell.CastInfo.SpellCastLaunchPosition.X, spell.CastInfo.SpellCastLaunchPosition.Z), current, trueCoords, HitResult.HIT_Normal, true);
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            var owner = spell.CastInfo.Owner;

            var ad = owner.Stats.AttackDamage.Total * 0.9f;
            var damage = 50 + (spell.CastInfo.SpellLevel * 30) + ad;

            if (!target.Status.HasFlag(StatusFlags.Stealthed))
            {
                if (owner.Team == TeamId.TEAM_BLUE)
                {
                    AddBuff("BlindMonkQOne", 3f, 1, spell, target, owner);
                }
                else
                {
                    AddBuff("BlindMonkQOneChaos", 3f, 1, spell, target, owner);
                }

                //AddBuff("BlindMonkSonicWave", 3f, 1, spell, target, owner);

                target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

                AddParticleTarget(owner, target, "blindMonk_Q_tar", target, bone: "C_BuffBone_Glb_Center_Loc", flags: 0);

                missile.SetToRemove();

                if (!target.IsDead && target is IObjAiBase ai)
                {
                    AddBuff("BlindMonkQManager", 3f, 1, spell, owner, ai);
                }
            }
            else
            {
                if (target is IChampion)
                {
                    if (owner.Team == TeamId.TEAM_BLUE)
                    {
                        AddBuff("BlindMonkQOne", 3f, 1, spell, target, owner);
                    }
                    else
                    {
                        AddBuff("BlindMonkQOneChaos", 3f, 1, spell, target, owner);
                    }

                    //AddBuff("BlindMonkSonicWave", 3f, 1, spell, target, owner);

                    target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

                    AddParticleTarget(owner, target, "blindMonk_Q_tar", target, bone: "C_BuffBone_Glb_Center_Loc", flags: 0);

                    missile.SetToRemove();

                    if (!target.IsDead && target is IObjAiBase ai)
                    {
                        AddBuff("BlindMonkQManager", 3f, 1, spell, owner, ai);
                    }
                }
                else
                {
                    // if (CanSeeTarget(owner, target))
                    if (owner.Team == TeamId.TEAM_BLUE)
                    {
                        AddBuff("BlindMonkQOne", 3f, 1, spell, target, owner);
                    }
                    else
                    {
                        AddBuff("BlindMonkQOneChaos", 3f, 1, spell, target, owner);
                    }

                    target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

                    AddParticleTarget(owner, target, "blindMonk_Q_tar", target, bone: "C_BuffBone_Glb_Center_Loc", flags: 0);

                    missile.SetToRemove();

                    if (!target.IsDead && target is IObjAiBase ai)
                    {
                        AddBuff("BlindMonkQManager", 3f, 1, spell, owner, ai);
                    }
                }
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

    public class BlindMonkQTwo : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            DoesntBreakShields = true,
            TriggersSpellCasts = true,
            NotSingleTargetSpell = false
        };

        IObjAiBase Owner;
        ISpell thisSpell;
        ISpellSector canCastSector;
        bool seal = true;
        string checkBuffName = "BlindMonkQOne";

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            Owner = owner;
            thisSpell = spell;
            seal = true;

            if (owner.Team != TeamId.TEAM_BLUE)
            {
                checkBuffName = "BlindMonkQOneChaos";
            }

            canCastSector = spell.CreateSpellSector(new SectorParameters
            {
                BindObject = Owner,
                Length = 1250f,
                SingleTick = false,
                CanHitSameTarget = false,
                CanHitSameTargetConsecutively = false,
                MaximumHits = 0,
                OverrideFlags = SpellDataFlags.AffectEnemies | SpellDataFlags.AffectNeutral | SpellDataFlags.AffectMinions | SpellDataFlags.AffectHeroes
            });

            SealSpellSlot(Owner, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, seal);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;

            string buffName = "BlindMonkQOne";
            if (owner.Team != TeamId.TEAM_BLUE)
            {
                buffName = "BlindMonkQOneChaos";
            }

            foreach (IAttackableUnit unit in GetUnitsInRange(owner.Position, 2000f, true))
            {
                if (spell.SpellData.IsValidTarget(owner, unit, SpellDataFlags.AffectEnemies | SpellDataFlags.AffectNeutral | SpellDataFlags.AffectMinions | SpellDataFlags.AffectHeroes))
                {
                    var buff = unit.GetBuffWithName(buffName);

                    if (buff != null && buff.SourceUnit == owner)
                    {
                        RemoveBuff(buff);
                        AddParticlePos(owner, "blindMonk_Q_resonatingStrike_02", owner.Position, owner.Position, bone: "C_BuffBone_Glb_Center_Loc", flags: 0);

                        if (unit is IObjAiBase ai)
                        {
                            AddBuff("BlindMonkQTwoDash", 2.5f, 1, spell, owner, ai);
                        }

                        RemoveBuff(owner, "BlindMonkQManager");
                    }
                }
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

        // TODO: Implement a CanCast function with a return value instead.
        public void OnUpdate(float diff)
        {
            if (Owner != null && canCastSector != null)
            {
                canCastSector.ObjectsHit.Clear();
                canCastSector.ExecuteTick();
                foreach (IGameObject obj in canCastSector.ObjectsHit)
                {
                    if (obj is IObjAiBase ai)
                    {
                        var buff = ai.GetBuffWithName(checkBuffName);
                        if (buff != null && buff.SourceUnit == Owner)
                        {
                            seal = false;
                            break;
                        }
                        else
                        {
                            seal = true;
                        }
                    }
                }

                SealSpellSlot(Owner, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, seal);
            }
        }
    }
}

using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;


namespace CharScripts
{
    internal class CharScriptKalista : ICharScript
    {
        IAttackableUnit lastTarget;

        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            ApiEventManager.OnPreAttack.AddListener(this, owner, OnPreAttack, false);
            ApiEventManager.OnLaunchAttack.AddListener(this, owner, OnLaunchAttack, false);
            ApiEventManager.OnMoveEnd.AddListener(this, owner, OnMoveEnd, false);
        }

        public void OnPreAttack(ISpell spell)
        {
            if (spell.CastInfo.Targets.Count > 0)
            {
                var target = spell.CastInfo.Targets[0].Unit;
                FaceDirection(target.Position, target, true);
            }
        }

        public void OnLaunchAttack(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            lastTarget = spell.CastInfo.Targets[0].Unit;

            if (!owner.IsPathEnded())
            {
                var baseDistance = 250f;

                // Boots increase speed. Tier 1 = +75, Tier 2 = +60, (speculative) Tier 3 = +45.
                // Boots increase dash distance. Tier 1 = +15, Tier 2 = +30, (speculative) Tier 3 = +45
                // TODO: Decrease speed based on percent movespeed mods (that are negative).
                var speed = 1025f;

                if (owner.Inventory.HasItemWithID(1001))
                {
                    speed += 75;
                    baseDistance += 15f;
                }
                else if (owner.Inventory.HasItemWithID(3006)
                    || owner.Inventory.HasItemWithID(3009)
                    || owner.Inventory.HasItemWithID(3020)
                    || owner.Inventory.HasItemWithID(3047)
                    || owner.Inventory.HasItemWithID(3111)
                    || owner.Inventory.HasItemWithID(3117)
                    || owner.Inventory.HasItemWithID(3158))
                {
                    speed += 135;
                    baseDistance += 30f;
                }
                else
                {
                    var boots = false;
                    for (int i = 3250; i < 3285; i++)
                    {
                        if (owner.Inventory.HasItemWithID(i))
                        {
                            boots = true;
                            break;
                        }
                    }

                    if (boots)
                    {
                        speed += 180;
                        baseDistance += 45f;
                    }
                }

                StopAnimation(owner, "", false, true, true);
                SetSpell(owner, "KalistaPassiveDashSpellActual", SpellSlotType.ExtraSlots, 0);
                //SpellCast(owner, 0, SpellSlotType.ExtraSlots, dashPos, dashPos, true, Vector2.Zero);

                // KalistaPassiveDashSpellActual code below

                // Minimum distance. No boots. Going towards attack direction.
                var dashPos = owner.Waypoints[owner.Waypoints.Count - 1];
                var angleDir = Extensions.UnitVectorToAngle(new Vector2(owner.Direction.X, owner.Direction.Z));

                // Decrease distance based on direction (backwards = farther, opposite behavior from Q dash)
                var finalDir = MathF.Abs(MathF.Abs(owner.Position.AngleTo(dashPos, owner.Position) - angleDir) - 180f);
                // TODO: Increase max distance based on boots Tier
                var angleMod = (finalDir / 180f);

                if (finalDir <= 165f && finalDir > 150f)
                {
                    angleMod /= 3;
                }
                else if (finalDir <= 150f && finalDir > 135f)
                {
                    angleMod /= 4;
                }
                else if (finalDir <= 135f && finalDir > 90f)
                {
                    angleMod /= 6;
                }
                else if (finalDir <= 90f)
                {
                    angleMod /= 9;
                }

                var distance = baseDistance - (100 * angleMod);
                LogDebug("Dashing. finalDir: " + finalDir + " distance: " + distance);

                FaceDirection(dashPos, owner, true);

                var targetPos = GetPointFromUnit(owner, distance);
                // TODO: Verify flags.
                PlayAnimation(owner, "Attack1_Dash", 0.0f, 0.0f, 1.0f, flags: AnimationFlags.Unknown8 | AnimationFlags.UniqueOverride);
                owner.CancelAutoAttack(false, true);
                ForceMovement(owner, "", targetPos, speed, 0.0f, 0.0f, 0.0f, true, ForceMovementType.FIRST_WALL_HIT, ForceMovementOrdersType.POSTPONE_CURRENT_ORDER, ForceMovementOrdersFacing.FACE_MOVEMENT_DIRECTION);
            }
        }

        public void OnMoveEnd(IAttackableUnit owner)
        {
            if (owner is IObjAiBase ai)
            {
                ai.SetTargetUnit(lastTarget);
            }
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}

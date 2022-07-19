using System.Numerics;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;

namespace Spells
{
    public class ZedShadowDash : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            AutoFaceDirection = false,
            TriggersSpellCasts = true
            // TODO
        };

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            AddBuff("ZedWPassiveBuff", 1.0f, 1, spell, owner, owner, true);
        }

        public void OnSpellPostCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner as Champion;
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            SpellCast(owner, 4, SpellSlotType.ExtraSlots, spellPos, spellPos, true, Vector2.Zero);
            PlayAnimation(owner, "Spell2_Cast", timeScale: 0.6f);
        }
    }

    public class ZedShadowDashMissile : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        Buff HandlerBuff;
        Minion Shadow;

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            HandlerBuff = AddBuff("ZedWHandler", 4.0f, 1, spell, owner, owner);
            AddBuff("ZedW2", 4.0f, 1, spell, owner, owner);

            if (Shadow != null)
            {
                var buff = Shadow.GetBuffWithName("ZedWShadowBuff");

                if (buff != null)
                {
                    buff.DeactivateBuff();
                }
            }

            var missile = spell.CreateSpellMissile(new MissileParameters
            {
                Type = MissileType.Circle,
                OverrideEndPosition = end
            });

            ApiEventManager.OnSpellMissileEnd.AddListener(this, missile, OnMissileEnd, true);
        }

        public void OnMissileEnd(SpellMissile missile)
        {
            if (HandlerBuff != null)
            {
                Shadow = (HandlerBuff.BuffScript as Buffs.ZedWHandler).ShadowSpawn();
            }
        }
    }

    public class ZedW2 : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
        };
    }
}

using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class SummonerRevive : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            if (!owner.IsDead)
            {
                return;
            }

            (owner as Champion).Respawn();
            AddBuff("SummonerReviveSpeedBoost", 12.0f, 1, spell, owner, owner);
            AddParticleTarget(owner, owner, "Global_SS_Revive", owner);
        }
    }
}


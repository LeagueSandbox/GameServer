using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class SummonerHaste : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            AddBuff("SummonerHasteBuff", 10.0f, 1, spell, owner, owner);
            var p1 = AddParticleTarget(owner, target, "Global_SS_Ghost", target);
            var p2 = AddParticleTarget(owner, target, "Global_SS_Ghost_cas", target);
            CreateTimer(10.0f, () =>
            {
                RemoveParticle(p1);
                RemoveParticle(p2);
            });
        }
    }
}


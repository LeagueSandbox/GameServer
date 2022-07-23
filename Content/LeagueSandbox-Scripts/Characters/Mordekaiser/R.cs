using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiEventManager;
using System.Numerics;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class MordekaiserChildrenOfTheGrave : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            OnPreDamagePriority = 10
        };

        Particle p;

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            p = AddParticleTarget(spell.CastInfo.Owner, target, "mordekeiser_cotg_tar", target, 10.400024f, flags: (FXFlags)32);
            Buff buff = AddBuff("MordekaiserChildrenOfTheGrave", 10.400024f, 1, spell, target, spell.CastInfo.Owner);

            OnBuffDeactivated.AddListener(this, buff, OnBuffRemoved, true);
        }

        public void OnBuffRemoved(Buff buff)
        {
            RemoveParticle(p);
        }
    }

    public class MordekaiserCotGGuide : BasePetController 
    {
    }
}

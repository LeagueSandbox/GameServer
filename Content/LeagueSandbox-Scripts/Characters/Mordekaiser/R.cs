using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiEventManager;
using System.Numerics;

namespace Spells
{
    public class MordekaiserChildrenOfTheGrave : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            OnPreDamagePriority = 10
        };

        IParticle p;

        public void OnSpellPreCast(IObjAIBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            p = AddParticleTarget(spell.CastInfo.Owner, target, "mordekeiser_cotg_tar", target, 10.400024f, flags: (FXFlags)32);
            IBuff buff = AddBuff("MordekaiserChildrenOfTheGrave", 10.400024f, 1, spell, target, spell.CastInfo.Owner);

            OnBuffDeactivated.AddListener(this, buff, OnBuffRemoved, true);
        }

        public void OnBuffRemoved(IBuff buff)
        {
            RemoveParticle(p);
        }
    }

    public class MordekaiserCotGGuide : BasePetController 
    {
    }
}

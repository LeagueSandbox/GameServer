using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace GameServerCore.Scripting.CSharp
{
    public interface IBuffGameScript
    {
        BuffScriptMetaData BuffMetaData { get; }
        StatsModifier StatsModifier { get; }

        void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
        }
        void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
        }
        void OnUpdate(float diff)
        {
        }
    }
}

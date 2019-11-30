using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public interface ISpellScript : IGameScript
    {
        void CooldownStarted(IChampion owner, ISpell spell);
        void CooldownEnded(IChampion owner, ISpell spell);
    }
}

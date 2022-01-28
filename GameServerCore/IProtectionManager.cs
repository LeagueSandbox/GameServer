using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace GameServerCore
{
    public interface IProtectionManager : IUpdate
    {
        void AddProtection(IAttackableUnit element, IAttackableUnit[] dependOnAll,
            IAttackableUnit[] dependOnSingle);

        void AddProtection(IAttackableUnit element, bool dependAll,
            params IAttackableUnit[] dependOn);

        void RemoveProtection(IAttackableUnit element);
        bool IsProtected(IAttackableUnit element);
        bool IsProtectionActive(IAttackableUnit element);
        void HandleFountainProtection(IChampion champion);
    }
}
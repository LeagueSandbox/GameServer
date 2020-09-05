using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace GameServerCore
{
    public interface IProtectionManager : IUpdate
    {
        public void AddProtection(IAttackableUnit element, IAttackableUnit[] dependOnAll,
            IAttackableUnit[] dependOnSingle);

        public void AddProtection(IAttackableUnit element, bool dependAll,
            params IAttackableUnit[] dependOn);

        public void RemoveProtection(IAttackableUnit element);
        public bool IsProtected(IAttackableUnit element);
    }
}
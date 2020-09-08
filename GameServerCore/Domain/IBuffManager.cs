using GameServerCore.Domain.GameObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerCore.Domain
{
    public interface IBuffManager
    {

        /// <summary>
        /// get buff count
        /// </summary>
        /// <returns>buff count</returns>
        int Count();

        void Add(IBuff buff);
        void Add(List<IBuff> buffs);

        List<IBuff> Get();
        IBuff Get(string buffName);
        IBuff Get(Predicate<IBuff> filter);

        List<IBuff> GetAll(string buffName);
        List<IBuff> GetAll(Predicate<IBuff> filter);

        bool Has(string buffName);
        bool Has(List<string> buffNames);
        bool Has(IBuff buff);
        bool Has(List<IBuff>buffs);
        bool Has(Predicate<IBuff> buffFilter);
        bool Has(List<Predicate<IBuff>> buffFilters);

        void Remove(IBuff buff);
        void Remove(string name);
        void Remove(List<IBuff> buffs);
        void Remove(Predicate<IBuff> buffFilter);
        void Remove(List<Predicate<IBuff>> buffFilters);

        byte GetSlot(IBuff buff);
        void RemoveSlot(IBuff buff);
    }
}

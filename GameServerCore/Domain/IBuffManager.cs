using GameServerCore.Domain.GameObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerCore.Domain
{
    public interface IBuffManager
    {

        /// <summary>
        /// get the number of buffs
        /// </summary>
        /// <returns>buff count</returns>
        int Count();

        /// <summary>
        /// add a buff
        /// </summary>
        /// <param name="buff">buff to add</param>
        void Add(IBuff buff);

        /// <summary>
        /// add multiple buffs
        /// </summary>
        /// <param name="buffs">list of buffs</param>
        void Add(List<IBuff> buffs);

        /// <summary>
        /// get all buffs
        /// </summary>
        /// <returns>list of all buffs</returns>
        List<IBuff> Get();

        /// <summary>
        /// get a buff by name
        /// </summary>
        /// <param name="buffName">name of buff</param>
        /// <returns>buff</returns>
        IBuff Get(string buffName);

        /// <summary>
        /// get a buff by filter
        /// </summary>
        /// <param name="filter">filter for buff</param>
        /// <returns>buff</returns>
        IBuff Get(Predicate<IBuff> filter);

        /// <summary>
        /// get all buffs with name
        /// </summary>
        /// <param name="buffName">buff name</param>
        /// <returns>buffs</returns>
        List<IBuff> GetAll(string buffName);

        /// <summary>
        /// get a filtered list of buffs
        /// </summary>
        /// <param name="filter">filter</param>
        /// <returns>buffs</returns>
        List<IBuff> GetAll(Predicate<IBuff> filter);

        /// <summary>
        /// check if unit has buff
        /// </summary>
        /// <param name="buffName">name of buff to check</param>
        /// <returns>unit has buff ?</returns>
        bool Has(string buffName);

        /// <summary>
        /// check if unit has buffs
        /// </summary>
        /// <param name="buffNames">list of buff names to check</param>
        /// <returns>unit has buffs ?</returns>
        bool Has(List<string> buffNames);

        /// <summary>
        /// check if unit has buff
        /// </summary>
        /// <param name="buff">buff to check</param>
        /// <returns>unít has buff ?</returns>
        bool Has(IBuff buff);

        /// <summary>
        /// check if unit has buffs
        /// </summary>
        /// <param name="buffs">buffs to check</param>
        /// <returns>unit has buffs ?</returns>
        bool Has(List<IBuff>buffs);

        /// <summary>
        /// check if unit has buff
        /// </summary>
        /// <param name="buffFilter">predicate filter of buff</param>
        /// <returns>unit has buff ?</returns>
        bool Has(Predicate<IBuff> buffFilter);

        /// <summary>
        /// check if unit has buffs
        /// </summary>
        /// <param name="buffFilters">predicate filters of buff</param>
        /// <returns>unit has buffs ?</returns>
        bool Has(List<Predicate<IBuff>> buffFilters);

        /// <summary>
        /// remove buff from unit
        /// </summary>
        /// <param name="buff">buff to remove</param>
        void Remove(IBuff buff);

        /// <summary>
        /// remove buff from unit
        /// </summary>
        /// <param name="name">name of buff to remove</param>
        void Remove(string name);

        /// <summary>
        /// removes multiple buffs from unit
        /// </summary>
        /// <param name="buffs">buffs to remove</param>
        void Remove(List<IBuff> buffs);

        /// <summary>
        /// remove buff from unit
        /// </summary>
        /// <param name="buffFilter">predicate filter for buff</param>
        void Remove(Predicate<IBuff> buffFilter);

        /// <summary>
        /// remove buffs from unit
        /// </summary>
        /// <param name="buffFilters">predicate filters for buff</param>
        void Remove(List<Predicate<IBuff>> buffFilters);

        /// <summary>
        /// get slot of the buff or a new one
        /// </summary>
        /// <param name="buff">buff of slot</param>
        /// <returns>slot of buff</returns>
        byte GetSlot(IBuff buff = null);

        /// <summary>
        /// remove slot of the buff
        /// </summary>
        /// <param name="buff">buff to remove slot from</param>
        void RemoveSlot(IBuff buff);
    }
}

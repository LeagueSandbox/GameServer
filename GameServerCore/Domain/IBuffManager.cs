using GameServerCore.Domain.GameObjects;
using Priority_Queue;
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
        void Add(IEnumerable<IBuff> buffs);

        /// <summary>
        /// get all buffs
        /// </summary>
        /// <returns>list of all buffs</returns>
        IEnumerable<IBuff> Get();

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
        IBuff Get(Func<IBuff, bool> filter);

        /// <summary>
        /// get all buffs with name
        /// </summary>
        /// <param name="buffName">buff name</param>
        /// <returns>buffs</returns>
        IEnumerable<IBuff> GetAll(string buffName);

        /// <summary>
        /// get a filtered list of buffs
        /// </summary>
        /// <param name="filter">filter</param>
        /// <returns>buffs</returns>
        IEnumerable<IBuff> GetAll(Func<IBuff, bool> filter);

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
        bool Has(IEnumerable<string> buffNames);

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
        bool Has(IEnumerable<IBuff> buffs);

        /// <summary>
        /// check if unit has buff
        /// </summary>
        /// <param name="buffFilter">predicate filter of buff</param>
        /// <returns>unit has buff ?</returns>
        bool Has(Func<IBuff, bool> buffFilter);

        /// <summary>
        /// check if unit has buffs
        /// </summary>
        /// <param name="buffFilters">predicate filters of buff</param>
        /// <returns>unit has buffs ?</returns>
        bool Has(IEnumerable<Func<IBuff, bool>> buffFilters);

        /// <summary>
        /// remove buff from unit
        /// </summary>
        /// <param name="buff">buff to remove</param>
        void Remove(IBuff buff);

        /// <summary>
        /// remove buff from unit
        /// </summary>
        /// <param name="name">name of buff to remove</param>
        void Remove(string name, bool removeAll = false);

        /// <summary>
        /// removes multiple buffs from unit
        /// </summary>
        /// <param name="buffs">buffs to remove</param>
        void Remove(IEnumerable<IBuff> buffs);

        /// <summary>
        /// remove buff from unit
        /// </summary>
        /// <param name="buffFilter">predicate filter for buff</param>
        void Remove(Func<IBuff, bool> buffFilter);

        /// <summary>
        /// remove buffs from unit
        /// </summary>
        /// <param name="buffFilters">predicate filters for buff</param>
        void Remove(IEnumerable<Func<IBuff, bool>> buffFilters);

        /// <summary>
        /// get slot of the buff or a new one
        /// </summary>
        /// <param name="buff">buff of slot</param>
        /// <param name="createNewOne">should create a new slot ?</param>
        /// <returns>slot of buff</returns>
        byte GetSlot(IBuff buff = null, bool createNewOne = false);

        /// <summary>
        /// remove slot of the buff
        /// </summary>
        /// <param name="buff">buff to remove slot from</param>
        void RemoveSlot(IBuff buff);

        /// <summary>
        /// removes elapsed buffs and updates current
        /// </summary>
        /// <param name="diff">time difference since last update</param>
        void Update(float diff);
    }
}

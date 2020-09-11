using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.GameObjects.Spells;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PacketDefinitions420.PacketDefinitions.S2C;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GameServerLib.GameObjects.Spells
{
    class BuffManager : IBuffManager
    {
        //private Dictionary<string, IBuff> _buffs;
        private List<IBuff> _buffQueue;
        private IBuff[] _slots;
        private Game _game;
        private IObjAiBase _target;

        public BuffManager(Game game, IObjAiBase target, IBuff[] initialBuffs = null)
        {
            _game = game;
            _target = target;

            _buffQueue = new List<IBuff>();
            _slots = new IBuff[256];

            if (initialBuffs != null)
            {
                Add(initialBuffs.ToList());
            }
        }

        protected void Replace(IBuff buff)
        {
            var previousBuff = Get(buff.Name);

            // remove old buff
            previousBuff.DeactivateBuff();
            Remove(buff.Name);
            RemoveSlot(buff);

            // replace old buff with new one in slots
            _slots[previousBuff.Slot] = buff;
            buff.SetSlot(previousBuff.Slot);

            // add new buff
            _buffQueue.Add(buff);

            // notify
            if (!buff.IsHidden)
            {
                _game.PacketNotifier.NotifyNPC_BuffReplace(buff);
            }

            buff.ActivateBuff();
        }

        protected void Renew(IBuff buff)
        {
            var actualBuff = Get(buff.Name);
            actualBuff.ResetTimeElapsed();

            if (!buff.IsHidden)
            {
                _game.PacketNotifier.NotifyNPC_BuffReplace(actualBuff);
            }

            _target.Stats.RemoveModifier(actualBuff.GetStatsModifier());
            actualBuff.ActivateBuff();
        }

        protected void StackOverlap(IBuff buff)
        {
            var actualBuff = Get(buff.Name);
            if(actualBuff.StackCount >= actualBuff.MaxStacks)
            {
                var tempBuffs = GetAll(buff.Name);
                var oldestBuff = tempBuffs.First();

                oldestBuff.DeactivateBuff();
                Remove(buff.Name);
                RemoveSlot(buff);

                tempBuffs = GetAll(buff.Name);

                _slots[oldestBuff.Slot] = tempBuffs.First();
                _buffQueue.Add(actualBuff);

                if (!buff.IsHidden)
                {
                    if (actualBuff.BuffType == BuffType.COUNTER)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(Get(buff.Name));
                    }
                    else
                    {
                        _game.PacketNotifier.NotifyNPC_BuffUpdateCount(buff, buff.Duration, buff.TimeElapsed);
                    }
                }

                buff.ActivateBuff();
            }
            else
            {
                actualBuff = Get(buff.Name);
                actualBuff.IncrementStackCount();

                GetAll(buff.Name).ToList().ForEach(x => x.SetStacks(actualBuff.StackCount));

                if (!buff.IsHidden)
                {
                    if (buff.BuffType == BuffType.COUNTER)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(Get(buff.Name));
                    }
                    else
                    {
                        _game.PacketNotifier.NotifyNPC_BuffUpdateCount(buff, buff.Duration, buff.TimeElapsed);
                    }
                }

                buff.ActivateBuff();
            }
        }

        protected void StackRenew(IBuff buff)
        {
            RemoveSlot(buff);

            var actualBuff = Get(buff.Name);

            actualBuff.ResetTimeElapsed();
            actualBuff.IncrementStackCount();

            if (!buff.IsHidden)
            {
                if (actualBuff.BuffType == BuffType.COUNTER)
                {
                    _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(actualBuff);
                }
                else
                {
                    _game.PacketNotifier.NotifyNPC_BuffUpdateCount(actualBuff, actualBuff.Duration, actualBuff.TimeElapsed);
                }
            }

            _target.Stats.RemoveModifier(actualBuff.GetStatsModifier()); // TODO: Replace with a better method that unloads -> reloads all data of a script
            actualBuff.ActivateBuff();
        }

        protected void AddNew(IBuff buff)
        {
            if(Has(buff.Name)) // TODO: is this realy needed ?
            {
                var actualBuff = Get(buff.Name);
                _buffQueue.Add(actualBuff);
                return;
            }

            _buffQueue.Add(buff);

            if (!buff.IsHidden)
            {
                _game.PacketNotifier.NotifyNPC_BuffAdd2(buff);
            }

            buff.ActivateBuff();
        }

        public void Add(IBuff buff)
        {
            // add a new buff
            if (_buffQueue.Where(x => x.Name.Equals(buff.Name)).Count() == 0)
            {
                AddNew(buff);
            }
            // handle overbuffing
            else
            {
                switch (buff.BuffAddType)
                {
                    case BuffAddType.REPLACE_EXISTING:
                        Replace(buff);
                        break;
                    case BuffAddType.RENEW_EXISTING:
                        Renew(buff);
                        break;
                    case BuffAddType.STACKS_AND_OVERLAPS:
                        StackOverlap(buff);
                        break;
                    case BuffAddType.STACKS_AND_RENEWS:
                        StackRenew(buff);
                        break;
                }
            }
        }

        public void Add(List<IBuff> buffs)
        {
            foreach (IBuff buff in buffs)
            {
                Add(buff);
            }
        }

        public int Count()
        {
            return _buffQueue.Count();
        }

        public IEnumerable<IBuff> Get()
        {
            return _buffQueue;
        }

        public IBuff Get(string buffName)
        {
            return _buffQueue.Where(x => x.Name.Equals(buffName)).First();
        }

        public IBuff Get(Func<IBuff, bool> filter)
        {
            return _buffQueue.Where(filter).First();
        }

        public IEnumerable<IBuff> GetAll(string buffName)
        {
            return _buffQueue.Where(buff => buff.Name.Equals(buffName));
        }

        public IEnumerable<IBuff> GetAll(Func<IBuff, bool> filter)
        {
            return _buffQueue.Where(filter);
        }

        public SimplePriorityQueue<IBuff, float> GetQueue()
        {
            var queue = new SimplePriorityQueue<IBuff, float>();
            foreach (var buff in _buffQueue)
                queue.Enqueue(buff, buff.Duration);

            return queue;
        }


        public bool Has(string buffName)
        {
            return _buffQueue.Where(x => x.IsBuffSame(buffName)).Count() > 0;
        }

        public bool Has(IEnumerable<string> buffNames)
        {
            foreach (var buffName in buffNames)
            {
                if (!Has(buffName))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Has(IBuff buff)
        {
            return Has(buff.Name);
        }

        public bool Has(IEnumerable<IBuff> buffs)
        {
            foreach (var buff in buffs)
            {
                if (!Has(buff))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Has(Func<IBuff, bool> buffFilter)
        {
            return _buffQueue.Where(buffFilter).Count() > 0;
        }

        public bool Has(IEnumerable<Func<IBuff, bool>> buffFilters)
        {
            foreach (var filter in buffFilters)
            {
                if (!Has(filter))
                {
                    return false;
                }
            }

            return true;
        }

        public void Remove(string buffName)
        {
            _buffQueue.Where(x => x.IsBuffSame(buffName)).ToList().ForEach(x => { x.DeactivateBuff(); _buffQueue.Remove(x); });
        }

        public void Remove(IBuff buff)
        {
            if(buff.BuffAddType == BuffAddType.STACKS_AND_OVERLAPS && buff.StackCount > 1)
            {
                buff.DecrementStackCount();

                Remove(buff.Name);
                RemoveSlot(buff);

                var tempBuffs = GetAll(buff.Name).ToList();
                tempBuffs.ForEach(tempBuff => tempBuff.SetStacks(buff.StackCount));

                _slots[buff.Slot] = tempBuffs[0];
                _buffQueue.Add(tempBuffs[0]);

                var newestBuff = tempBuffs.Last();

                if (!buff.IsHidden)
                {
                    if (buff.BuffType == BuffType.COUNTER)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(Get(buff.Name));
                    }                        
                    else
                    {
                        if(buff.StackCount == 1)
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateCount(newestBuff, buff.Duration - newestBuff.TimeElapsed, newestBuff.TimeElapsed);
                        }
                        else
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateCountGroup(_target, tempBuffs, buff.Duration - newestBuff.TimeElapsed, newestBuff.TimeElapsed);
                        }
                    }
                }
            }
            else
            {
                _buffQueue.Where(buff => buff.Elapsed()).ToList().ForEach(x => _buffQueue.Remove(x));
                Remove(buff.Name);
                RemoveSlot(buff);

                if(!buff.IsHidden)
                {
                    _game.PacketNotifier.NotifyNPC_BuffRemove2(buff);
                }
            }
        }

        public void Remove(IEnumerable<IBuff> buffs)
        {
            foreach (var buff in buffs)
            {
                Remove(buff);
            }
        }

        public void Remove(Func<IBuff, bool> buffFilter)
        {
            var buffsToRemove = _buffQueue.Where(buffFilter);

            foreach (var buffToRemove in buffsToRemove)
            {
                Remove(buffToRemove);
            }
        }

        public void Remove(IEnumerable<Func<IBuff, bool>> buffFilters)
        {
            foreach (var filter in buffFilters)
            {
                Remove(filter);
            }
        }

        public byte GetSlot(IBuff buff = null, bool createNew = false)
        {
            IBuff tempBuff = null;
            if(createNew)
            {
                tempBuff = buff;
                buff = null;
            }

            for (byte i = 1; i < _slots.Length; i++)
            {
                if (_slots[i] == buff)
                {
                    if (createNew)
                        _slots[i] = tempBuff;
                    return i;
                }
            }

            throw new Exception($"No slot found with {buff.ToString()}");
        }
        public void RemoveSlot(IBuff buff)
        {
            var slot = GetSlot(buff);
            _slots[slot] = null;
        }
    }
}

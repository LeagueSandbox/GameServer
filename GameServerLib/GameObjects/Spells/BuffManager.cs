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
        private SimplePriorityQueue<IBuff> _buffQueue;
        private IBuff[] _slots;
        private Game _game;
        private IObjAiBase _target;

        public BuffManager(Game game, IObjAiBase target, IBuff[] initialBuffs = null)
        {
            _game = game;
            _target = target;

            _buffQueue = new SimplePriorityQueue<IBuff>();
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
            Remove(buff);
            RemoveSlot(buff);

            // replace old buff with new one in slots
            _slots[previousBuff.Slot] = buff;
            buff.SetSlot(previousBuff.Slot);

            // add new buff
            _buffQueue.Enqueue(buff, buff.Duration);

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
                Remove(buff);
                RemoveSlot(buff);

                tempBuffs = GetAll(buff.Name);

                _slots[oldestBuff.Slot] = actualBuff;
                _buffQueue.Enqueue(actualBuff, actualBuff.Duration);

                if (!buff.IsHidden)
                {
                    if (actualBuff.BuffType == BuffType.COUNTER)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(actualBuff);
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
                _buffQueue.Enqueue(actualBuff, actualBuff.Duration);
                return;
            }

            _buffQueue.Enqueue(buff, buff.Duration);

            if (!buff.IsHidden)
            {
                _game.PacketNotifier.NotifyNPC_BuffAdd2(buff);
            }

            buff.ActivateBuff();
        }

        public void Add(IBuff buff)
        {
            // add a new buff
            if (_buffQueue.ToList().FindAll(x => x.Name.Equals(buff.Name)).Count == 0)
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
            return _buffQueue.Count;
        }

        public IEnumerable<IBuff> Get()
        {
            return _buffQueue.ToList();
        }

        public IBuff Get(string buffName)
        {
            return _buffQueue.Where(x => x.Name.Equals(buffName)).FirstOrDefault();
        }

        public IBuff Get(Func<IBuff, bool> filter)
        {
            return _buffQueue.Where(filter).FirstOrDefault();
        }

        public IEnumerable<IBuff> GetAll(string buffName)
        {
            return _buffQueue.Where(buff => buff.IsBuffSame(buffName));
        }

        public IEnumerable<IBuff> GetAll(Func<IBuff, bool> filter)
        {
            return _buffQueue.Where(filter);
        }

        public SimplePriorityQueue<IBuff> GetQueue()
        {
            return _buffQueue;
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
            _buffQueue.Where(x => x.IsBuffSame(buffName)).ToList().ForEach(x => _buffQueue.Remove(x));
        }

        public void Remove(IBuff buff)
        {
            if(buff.BuffAddType == BuffAddType.STACKS_AND_OVERLAPS && buff.StackCount > 1)
            {
                buff.DecrementStackCount();

                Remove(buff);
                RemoveSlot(buff);

                var tempBuffs = GetAll(buff.Name).ToList();
                tempBuffs.ForEach(tempBuff => tempBuff.SetStacks(buff.StackCount));

                _slots[buff.Slot] = tempBuffs[0];
                _buffQueue.Enqueue(tempBuffs[0], tempBuffs[0].Duration);

                var newestBuff = tempBuffs[tempBuffs.Count - 1];

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

        public byte GetSlot(IBuff buff = null)
        {
            for (byte i = 1; i < _slots.Length; i++)
            {
                if (_slots[i] == buff)
                {
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

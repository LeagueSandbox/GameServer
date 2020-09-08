using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PacketDefinitions420.PacketDefinitions.S2C;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GameServerLib.GameObjects.Spells
{
    class BuffManager : IBuffManager
    {
        // TODO: maybe change to a List<IBuff> and access names per LINQ
        private Dictionary<string, IBuff> _buffs;
        private IBuff[] _slots;
        private Game _game;
        private IObjAiBase _target;

        public BuffManager(Game game, IObjAiBase target, IBuff[] initialBuffs = null)
        {
            _game = game;
            _target = target;

            _buffs = new Dictionary<string, IBuff>();
            _slots = new IBuff[256];

            if (initialBuffs != null)
            {
                Add(initialBuffs.ToList());
            }
        }

        protected void Replace(IBuff buff)
        {
            var previousBuff = _buffs[buff.Name];

            previousBuff.DeactivateBuff();
            Remove(buff);
            RemoveSlot(buff);

            _slots[previousBuff.Slot] = buff;
            buff.SetSlot(previousBuff.Slot);

            _buffs.Add(buff.Name, buff);

            if (!buff.IsHidden)
            {
                _game.PacketNotifier.NotifyNPC_BuffReplace(buff);
            }

            buff.ActivateBuff();
        }

        protected void Renew(IBuff buff)
        {
            _buffs[buff.Name].ResetTimeElapsed();

            if (!buff.IsHidden)
            {
                _game.PacketNotifier.NotifyNPC_BuffReplace(_buffs[buff.Name]);
            }

            _target.Stats.RemoveModifier(_buffs[buff.Name].GetStatsModifier());
            _buffs[buff.Name].ActivateBuff();
        }

        protected void StackOverlap(IBuff buff)
        {
            if(_buffs[buff.Name].StackCount >= _buffs[buff.Name].MaxStacks)
            {
                var tempBuffs = GetAll(buff.Name);
                var oldestBuff = tempBuffs[0];

                oldestBuff.DeactivateBuff();
                Remove(buff);
                RemoveSlot(buff);

                tempBuffs = GetAll(buff.Name);

                _slots[oldestBuff.Slot] = tempBuffs[0];
                _buffs.Add(oldestBuff.Name, tempBuffs[0]);

                if (!buff.IsHidden)
                {
                    if (_buffs[buff.Name].BuffType == BuffType.COUNTER)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(_buffs[buff.Name]);
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

            _buffs[buff.Name].ResetTimeElapsed();
            _buffs[buff.Name].IncrementStackCount();

            if (!buff.IsHidden)
            {
                if (_buffs[buff.Name].BuffType == BuffType.COUNTER)
                {
                    _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(_buffs[buff.Name]);
                }
                else
                {
                    _game.PacketNotifier.NotifyNPC_BuffUpdateCount(_buffs[buff.Name], _buffs[buff.Name].Duration, _buffs[buff.Name].TimeElapsed);
                }
            }

            _target.Stats.RemoveModifier(_buffs[buff.Name].GetStatsModifier()); // TODO: Replace with a better method that unloads -> reloads all data of a script
            _buffs[buff.Name].ActivateBuff();
        }

        protected void AddNew(IBuff buff)
        {
            if(Has(buff.Name)) // TODO: is this realy needed ?
            {
                var actualBuff = Get(buff.Name);
                _buffs.Add(buff.Name, actualBuff);
                return;
            }

            _buffs.Add(buff.Name, buff);

            if (!buff.IsHidden)
            {
                _game.PacketNotifier.NotifyNPC_BuffAdd2(buff);
            }

            buff.ActivateBuff();
        }

        public void Add(IBuff buff)
        {
            // add a new buff
            if (!_buffs.ContainsKey(buff.Name))
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
            return _buffs.Count;
        }

        public List<IBuff> Get()
        {
            return _buffs.Values.ToList();
        }

        public IBuff Get(string buffName)
        {
            return _buffs[buffName];
        }

        public IBuff Get(Predicate<IBuff> filter)
        {
            return _buffs.Values.ToList().Find(filter);
        }

        public List<IBuff> GetAll(string buffName)
        {
            return _buffs.Values.ToList().FindAll(buff => buff.IsBuffSame(buffName));
        }

        public List<IBuff> GetAll(Predicate<IBuff> filter)
        {
            return _buffs.Values.ToList().FindAll(filter);
        }

        public byte GetSlot(IBuff buff = null)
        {
            for(byte i = 1; i < _slots.Length; i++)
            {
                if (_slots[i] == buff)
                {
                    return i;
                }
            }

            throw new Exception($"No slot found with {buff.ToString()}");
        }

        public bool Has(string buffName)
        {
            return _buffs.Values.Where(x => x.IsBuffSame(buffName)).Count() > 0;
        }

        public bool Has(List<string> buffNames)
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

        public bool Has(List<IBuff> buffs)
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

        public bool Has(Predicate<IBuff> buffFilter)
        {
            return _buffs.Values.ToList().FindAll(buffFilter).Count > 0;
        }

        public bool Has(List<Predicate<IBuff>> buffFilters)
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

        public void Remove(IBuff buff)
        {
            if(buff.BuffAddType == BuffAddType.STACKS_AND_OVERLAPS && buff.StackCount > 1)
            {
                buff.DecrementStackCount();

                Remove(buff.Name);
                RemoveSlot(buff);

                var tempBuffs = GetAll(buff.Name);
                tempBuffs.ForEach(tempBuff => tempBuff.SetStacks(buff.StackCount));

                _slots[buff.Slot] = tempBuffs[0];
                _buffs.Add(buff.Name, tempBuffs[0]);

                var newestBuff = tempBuffs[tempBuffs.Count - 1];

                if (!buff.IsHidden)
                {
                    if (buff.BuffType == BuffType.COUNTER)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(_buffs[buff.Name]);
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
                _buffs.Values.ToList().RemoveAll(buff => buff.Elapsed());
                Remove(buff.Name);
                RemoveSlot(buff);

                if(!buff.IsHidden)
                {
                    _game.PacketNotifier.NotifyNPC_BuffRemove2(buff);
                }
            }
        }

        public void Remove(string buffName)
        {
            _buffs.Remove(buffName);
        }

        public void Remove(List<IBuff> buffs)
        {
            foreach (var buff in buffs)
            {
                Remove(buff);
            }
        }

        public void Remove(Predicate<IBuff> buffFilter)
        {
            var buffsToRemove = _buffs.Values.ToList().FindAll(buffFilter);

            foreach (var buffToRemove in buffsToRemove)
            {
                Remove(buffToRemove);
            }
        }

        public void Remove(List<Predicate<IBuff>> buffFilters)
        {
            foreach (var filter in buffFilters)
            {
                Remove(filter);
            }
        }

        public void RemoveSlot(IBuff buff)
        {
            var slot = GetSlot(buff);
            _slots[slot] = null;
        }
    }
}

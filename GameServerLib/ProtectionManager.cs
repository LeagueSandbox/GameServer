﻿using System.Collections.Generic;
using System.Linq;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer
{
    public class ProtectionManager : IProtectionManager
    {
        private Dictionary<IAttackableUnit, IAttackableUnit[]> _dependOnAll = new Dictionary<IAttackableUnit, IAttackableUnit[]>();
        private Dictionary<IAttackableUnit, IAttackableUnit[]> _dependOnSingle = new Dictionary<IAttackableUnit, IAttackableUnit[]>();
        private List<IAttackableUnit> _protectedElements = new List<IAttackableUnit>();
        private List<IAttackableUnit> _hasProtectionElements = new List<IAttackableUnit>();

        public ProtectionManager()
        {
            
        }

        public void AddProtection(IAttackableUnit element, IAttackableUnit[] dependOnAll,
            IAttackableUnit[] dependOnSingle)
        {
            _dependOnAll.Add(element, dependOnAll);
            _dependOnSingle.Add(element, dependOnSingle);
            _protectedElements.Add(element);
        }
        
        public void AddProtection(IAttackableUnit element, bool dependAll,
            params IAttackableUnit[] dependOn)
        {
            if (dependAll)
            {
                _dependOnAll.Add(element, dependOn);
            }
            else
            {
                _dependOnSingle.Add(element, dependOn);
            }
            _protectedElements.Add(element);
        }

        public void RemoveProtection(IAttackableUnit element)
        {
            _dependOnAll.Remove(element);
            _dependOnSingle.Remove(element);
            _protectedElements.Remove(element);
        }

        public bool IsProtected(IAttackableUnit element)
        {
            return _protectedElements.Contains(element);
        }

        public void Update(float diff)
        {
            foreach (var element in _protectedElements)
            {
                if (_dependOnAll.ContainsKey(element) || _dependOnSingle.ContainsKey(element))
                {
                    int destroyedAllCount = 0;
                    int destroyedSingleCount = 0;
                    if (_dependOnAll.ContainsKey(element))
                    {
                        foreach (var el in _dependOnAll[element])
                        {
                            if (el.IsDead) destroyedAllCount++;
                        }
                    }

                    if (_dependOnSingle.ContainsKey(element))
                    {
                        foreach (var el in _dependOnSingle[element])
                        {
                            if (el.IsDead)
                            {
                                destroyedSingleCount++;
                                break;
                            }
                        }
                    }

                    if ((!_dependOnAll.ContainsKey(element) || _dependOnAll[element].Length == 0 ||
                         destroyedAllCount == _dependOnAll[element].Length) &&
                        (!_dependOnSingle.ContainsKey(element) || _dependOnSingle[element].Length == 0 ||
                         destroyedSingleCount >= 1))
                    {
                        if (_hasProtectionElements.Contains(element))
                        {
                            element.SetIsTargetableToTeam(element.Team == TeamId.TEAM_BLUE ? TeamId.TEAM_PURPLE : TeamId.TEAM_BLUE, true);
                            _hasProtectionElements.Remove(element);
                        }
                    }
                    else
                    {
                        if (!_hasProtectionElements.Contains(element))
                        {
                            element.SetIsTargetableToTeam(element.Team == TeamId.TEAM_BLUE ? TeamId.TEAM_PURPLE : TeamId.TEAM_BLUE, false);
                            _hasProtectionElements.Add(element);
                        }
                    }
                }
            }
        }
    }
}
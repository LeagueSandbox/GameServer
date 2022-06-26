using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Logging;
using log4net;
using System;
using System.Collections.Generic;

/*
 * Possible Events:
 * Events that are always performed are accounted for in-script, no event handling needed. EX: Spell calls spellscript.OnActivate()
[OnActivate] - buffs and spells (always performed)
[OnAddPAR]
[OnAllowAddBuff]
[OnAssist]
[OnAssistUnit]
[OnBeingDodged]
[OnBeingHit]
[OnBeingSpellHit]
[OnCanCast]
[OnCollision]
[OnCollisionTerrain]
[OnDeactivate] - buffs and spells (always performed)
[OnDealDamage]
[OnDeath]
[OnDisconnect]
[OnDodge]
[OnHeal]
[OnHitUnit]
[OnKill]
[OnKillUnit]
[OnLaunchAttack]
[OnLaunchMissile]
[OnLevelUp]
[OnLevelUpSpell]
[OnMiss]
[OnMissileEnd]
[OnMissileUpdate]
[OnMoveEnd]
[OnMoveFailure]
[OnMoveSuccess]
[OnNearbyDeath]
[OnPreAttack]
[OnPreDamage]
[OnPreDealDamage]
[OnPreMitigationDamage]
[OnPreTakeDamage]
[OnReconnect]
[OnResurrect]
[OnSpellCast] - start casting
[OnSpellChannel] - start channeling
[OnSpellChannelCancel] - abrupt stop channeling
[OnSpellPostCast] - finish casting
[OnSpellPostChannel] - finish channeling
[OnSpellPreCast] - setup cast info before casting (always performed)
[OnSpellHit] - "ApplyEffects" function in Spell.
[OnTakeDamage]
[OnUpdateActions] - move order probably
[OnUpdateAmmo]
[OnUpdateStats]
[OnZombie]
 */

namespace LeagueSandbox.GameServer.API
{
    public static class ApiEventManager
    {
        private static Game _game;
        private static ILog _logger;
        private static List<DispatcherBase> _dispatchers = new List<DispatcherBase>();

        internal static void SetGame(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
        }

        public static void RemoveAllListenersForOwner(object owner)
        {
            foreach(var dispatcher in _dispatchers)
            {
                dispatcher.RemoveListener(owner);
            }
        }

        // Unused
        public static Dispatcher<IAttackableUnit, IAttackableUnit> OnAddPAR
                = new Dispatcher<IAttackableUnit, IAttackableUnit>();
        public static ConditionDispatcher<IAttackableUnit, IAttackableUnit, IBuff> OnAllowAddBuff
                = new ConditionDispatcher<IAttackableUnit, IAttackableUnit, IBuff>();
        public static Dispatcher<IAttackableUnit, IAttackableUnit> OnBeingHit
                = new Dispatcher<IAttackableUnit, IAttackableUnit>();
        public static Dispatcher<IAttackableUnit, ISpell, ISpellMissile, ISpellSector> OnBeingSpellHit
                = new Dispatcher<IAttackableUnit, ISpell, ISpellMissile, ISpellSector>();
        public static Dispatcher<IBuff> OnBuffDeactivated
                = new Dispatcher<IBuff>();
        public static ConditionDispatcher<IAttackableUnit, ISpell> OnCanCast
                = new ConditionDispatcher<IAttackableUnit, ISpell>();
        public static Dispatcher<IGameObject, IGameObject> OnCollision
                = new Dispatcher<IGameObject, IGameObject>();
        public static Dispatcher<IGameObject> OnCollisionTerrain
                = new Dispatcher<IGameObject>();
        public static Dispatcher<ISpell, ISpellSector> OnCreateSector
                = new Dispatcher<ISpell, ISpellSector>();
        public static DataOnlyDispatcher<IAttackableUnit, IDamageData> OnDealDamage
                = new DataOnlyDispatcher<IAttackableUnit, IDamageData>();
        public static DataOnlyDispatcher<IAttackableUnit, IDeathData> OnDeath
                = new DataOnlyDispatcher<IAttackableUnit, IDeathData>();
        public static DataOnlyDispatcher<IObjAiBase, IDamageData> OnHitUnit
                = new DataOnlyDispatcher<IObjAiBase, IDamageData>();
        public static DataOnlyDispatcher<IChampion, IScoreData> OnIncrementChampionScore
                = new DataOnlyDispatcher<IChampion, IScoreData>();
        public static DataOnlyDispatcher<IAttackableUnit, IDeathData> OnKill
                = new DataOnlyDispatcher<IAttackableUnit, IDeathData>();
        public static DataOnlyDispatcher<IAttackableUnit, IDeathData> OnKillUnit
                = new DataOnlyDispatcher<IAttackableUnit, IDeathData>();
        public static DataOnlyDispatcher<IObjAiBase, ISpell> OnLaunchAttack
                = new DataOnlyDispatcher<IObjAiBase, ISpell>();
        /// <summary>
        /// Called immediately after the rocket is added to the scene. *NOTE*: At the time of the call, the rocket has not yet been spawned for players.
        /// <summary>
        public static Dispatcher<ISpell, ISpellMissile> OnLaunchMissile
                = new Dispatcher<ISpell, ISpellMissile>();
        public static Dispatcher<IAttackableUnit> OnLevelUp
                = new Dispatcher<IAttackableUnit>();
        public static Dispatcher<ISpell> OnLevelUpSpell
                = new Dispatcher<ISpell>();
        public static Dispatcher<IAttackableUnit> OnMoveEnd
                = new Dispatcher<IAttackableUnit>();
        public static Dispatcher<IAttackableUnit> OnMoveFailure
                = new Dispatcher<IAttackableUnit>();
        public static Dispatcher<IAttackableUnit> OnMoveSuccess
                = new Dispatcher<IAttackableUnit>();
        public static DataOnlyDispatcher<IObjAiBase, ISpell> OnPreAttack
                = new DataOnlyDispatcher<IObjAiBase, ISpell>();
        public static DataOnlyDispatcher<IAttackableUnit, IDamageData> OnPreDealDamage
                = new DataOnlyDispatcher<IAttackableUnit, IDamageData>();
        public static DataOnlyDispatcher<IAttackableUnit, IDamageData> OnPreTakeDamage
                = new DataOnlyDispatcher<IAttackableUnit, IDamageData>();
        public static Dispatcher<IObjAiBase> OnResurrect
                = new Dispatcher<IObjAiBase>();
        public static Dispatcher<ISpell> OnSpellCast
                = new Dispatcher<ISpell>();
        public static Dispatcher<ISpell> OnSpellChannel
                = new Dispatcher<ISpell>();
        public static Dispatcher<ISpell, ChannelingStopSource> OnSpellChannelCancel
                = new Dispatcher<ISpell, ChannelingStopSource>();
        public static Dispatcher<ISpell, IAttackableUnit, ISpellMissile, ISpellSector> OnSpellHit
                = new Dispatcher<ISpell, IAttackableUnit, ISpellMissile, ISpellSector>();
        public static Dispatcher<ISpellMissile> OnSpellMissileEnd
                = new Dispatcher<ISpellMissile>();
        public static Dispatcher<ISpellMissile, IAttackableUnit> OnSpellMissileHit
                = new Dispatcher<ISpellMissile, IAttackableUnit>();
        public static Dispatcher<ISpellMissile, float> OnSpellMissileUpdate
                = new Dispatcher<ISpellMissile, float>();
        public static Dispatcher<ISpell> OnSpellPostCast
                = new Dispatcher<ISpell>();
        public static Dispatcher<ISpell> OnSpellPostChannel
                = new Dispatcher<ISpell>();
        public static Dispatcher<ISpellSector, IAttackableUnit> OnSpellSectorHit
                = new Dispatcher<ISpellSector, IAttackableUnit>();
        public static DataOnlyDispatcher<IAttackableUnit, IDamageData> OnTakeDamage
                = new DataOnlyDispatcher<IAttackableUnit, IDamageData>();
        public static DataOnlyDispatcher<IObjAiBase, IAttackableUnit> OnTargetLost
                = new DataOnlyDispatcher<IObjAiBase, IAttackableUnit>();
        public static Dispatcher<IAttackableUnit, IBuff> OnUnitBuffDeactivated
                = new Dispatcher<IAttackableUnit, IBuff>();
        // TODO: Handle crowd control the same as normal dashes.
        public static Dispatcher<IAttackableUnit> OnUnitCrowdControlled
                = new Dispatcher<IAttackableUnit>();
        // TODO: Change to OnMoveSuccess and change where Publish is called internally to reflect the name.
        public static ConditionDispatcher<IObjAiBase, OrderType> OnUnitUpdateMoveOrder
                = new ConditionDispatcher<IObjAiBase, OrderType>();
        public static Dispatcher<IAttackableUnit, float> OnUpdateStats
                = new Dispatcher<IAttackableUnit, float>();

        public abstract class DispatcherBase
        {
            public DispatcherBase()
            {
                _dispatchers.Add(this);
            }
            public abstract void RemoveListener(object owner);
        }

        public abstract class DispatcherBase<Source, CBType>: DispatcherBase
        {
            protected readonly List<Tuple<object, Source, CBType, bool>> _listeners
                    = new List<Tuple<object, Source, CBType, bool>>();

            public void AddListener(object owner, Source source, CBType callback, bool singleInstance = false)
            {
                _listeners.Add(
                    new Tuple<object, Source, CBType, bool>(owner, source, callback, singleInstance)
                );
            }

            public void RemoveListener(object owner, Source source)
            {
                _listeners.RemoveAll(listener => listener.Item1 == owner && listener.Item2.Equals(source));
            }

            public override void RemoveListener(object owner)
            {
                _listeners.RemoveAll(listener => listener.Item1 == owner);
            }
        }

        public abstract class VariableDispatcherBase<Source, Data, CBType>: VariableDispatcherBase<Source, CBType>
        {
            protected Data _data;
            public void Publish(Source source, Data data)
            {
                _data = data;
                base.Publish(source);
            }
        }

        public abstract class VariableDispatcherBase<Source, CBType>: DispatcherBase<Source, CBType>
        {
            protected Source _source;
            protected abstract void Call(CBType callback);
            protected void Publish(Source source)
            {
                _source = source;

                for (int i = _listeners.Count - 1; i >= 0; i--)
                {
                    if (_listeners[i].Item2.Equals(source))
                    {
                        var listener = _listeners[i];
                        Call(listener.Item3);

                        if (listener.Item4)
                        {
                            _listeners.Remove(listener);
                        }
                    }
                }
            }
        }

        public abstract class ConditionDispatcherBase<Source, Data, CBType>: DispatcherBase<Source, CBType>
        {
            protected Source _source;
            protected Data _data;
            protected abstract bool Call(CBType callback);
            public bool Publish(Source source, Data data)
            {
                _source = source;
                _data = data;

                bool returnVal = true;
                for (int i = _listeners.Count - 1; i >= 0; i--)
                {
                    if (_listeners[i].Item2.Equals(source))
                    {
                        var listener = _listeners[i];
                        returnVal = returnVal && Call(listener.Item3);

                        if (listener.Item4)
                        {
                            _listeners.Remove(listener);
                        }
                    }
                }
                return returnVal;
            }
        }

        public class Dispatcher<Source>: VariableDispatcherBase<Source, Action<Source>>
        {
            public new void Publish(Source source)
            {
                base.Publish(source);
            }
            protected override void Call(Action<Source> callback)
            {
                callback(_source);
            }
        }

        public class Dispatcher<Source, Data>: VariableDispatcherBase<Source, Data, Action<Source, Data>>
        {
            protected override void Call(Action<Source, Data> callback)
            {
                callback(_source, _data);
            }
        }

        public class DataOnlyDispatcher<Source, Data>: VariableDispatcherBase<Source, Data, Action<Data>>
        {
            protected override void Call(Action<Data> callback)
            {
                callback(_data);
            }
        }

        public class Dispatcher<Source, D1, D2, D3>: VariableDispatcherBase<Source, (D1, D2, D3), Action<Source, D1, D2, D3>>
        {
            protected override void Call(Action<Source, D1, D2, D3> callback)
            {
                callback(_source, _data.Item1, _data.Item2, _data.Item3);
            }
        }

        public class Dispatcher<Source, D1, D2, D3, D4>: VariableDispatcherBase<Source, (D1, D2, D3, D4), Action<Source, D1, D2, D3, D4>>
        {
            protected override void Call(Action<Source, D1, D2, D3, D4> callback)
            {
                callback(_source, _data.Item1, _data.Item2, _data.Item3, _data.Item4);
            }
        }

        public class ConditionDispatcher<Source, Data>: ConditionDispatcherBase<Source, Data, Func<Source, Data, bool>>
        {
            protected override bool Call(Func<Source, Data, bool> callback)
            {
                return callback(_source, _data);
            }
        }

        public class ConditionDispatcher<Source, D1, D2>: ConditionDispatcherBase<Source, (D1, D2), Func<Source, D1, D2, bool>>
        {
            protected override bool Call(Func<Source, D1, D2, bool> callback)
            {
                return callback(_source, _data.Item1, _data.Item2);
            }
        }
    }

    // TODO: Make listeners support removal at any point in code execution.
}
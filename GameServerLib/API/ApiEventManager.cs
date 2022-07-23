using LeagueSandbox.GameServer.GameObjects;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
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
        private static ILog _logger = LoggerProvider.GetLogger();
        private static List<DispatcherBase> _dispatchers = new List<DispatcherBase>();

        internal static void SetGame(Game game)
        {
            _game = game;
        }

        public static void RemoveAllListenersForOwner(object owner)
        {
            foreach (var dispatcher in _dispatchers)
            {
                dispatcher.RemoveListener(owner);
            }
        }

        // Unused
        public static Dispatcher<AttackableUnit, AttackableUnit> OnAddPAR
                = new Dispatcher<AttackableUnit, AttackableUnit>();
        public static ConditionDispatcher<AttackableUnit, AttackableUnit, Buff> OnAllowAddBuff
                = new ConditionDispatcher<AttackableUnit, AttackableUnit, Buff>();
        public static Dispatcher<AttackableUnit, AttackableUnit> OnBeingHit
                = new Dispatcher<AttackableUnit, AttackableUnit>();
        public static Dispatcher<AttackableUnit, Spell, SpellMissile, SpellSector> OnBeingSpellHit
                = new Dispatcher<AttackableUnit, Spell, SpellMissile, SpellSector>();
        public static Dispatcher<Buff> OnBuffDeactivated
                = new Dispatcher<Buff>();
        public static ConditionDispatcher<AttackableUnit, Spell> OnCanCast
                = new ConditionDispatcher<AttackableUnit, Spell>();
        public static Dispatcher<GameObject, GameObject> OnCollision
                = new Dispatcher<GameObject, GameObject>();
        public static Dispatcher<GameObject> OnCollisionTerrain
                = new Dispatcher<GameObject>();
        public static Dispatcher<Spell, SpellSector> OnCreateSector
                = new Dispatcher<Spell, SpellSector>();
        public static DataOnlyDispatcher<AttackableUnit, DamageData> OnDealDamage
                = new DataOnlyDispatcher<AttackableUnit, DamageData>();
        public static DataOnlyDispatcher<AttackableUnit, DeathData> OnDeath
                = new DataOnlyDispatcher<AttackableUnit, DeathData>();
        public static DataOnlyDispatcher<ObjAIBase, DamageData> OnHitUnit
                = new DataOnlyDispatcher<ObjAIBase, DamageData>();
        public static DataOnlyDispatcher<Champion, ScoreData> OnIncrementChampionScore
                = new DataOnlyDispatcher<Champion, ScoreData>();
        public static DataOnlyDispatcher<AttackableUnit, DeathData> OnKill
                = new DataOnlyDispatcher<AttackableUnit, DeathData>();
        public static DataOnlyDispatcher<AttackableUnit, DeathData> OnKillUnit
                = new DataOnlyDispatcher<AttackableUnit, DeathData>();
        public static DataOnlyDispatcher<ObjAIBase, Spell> OnLaunchAttack
                = new DataOnlyDispatcher<ObjAIBase, Spell>();
        /// <summary>
        /// Called immediately after the rocket is added to the scene. *NOTE*: At the time of the call, the rocket has not yet been spawned for players.
        /// <summary>
        public static Dispatcher<Spell, SpellMissile> OnLaunchMissile
                = new Dispatcher<Spell, SpellMissile>();
        public static Dispatcher<AttackableUnit> OnLevelUp
                = new Dispatcher<AttackableUnit>();
        public static Dispatcher<Spell> OnLevelUpSpell
                = new Dispatcher<Spell>();
        public static Dispatcher<AttackableUnit> OnMoveEnd
                = new Dispatcher<AttackableUnit>();
        public static Dispatcher<AttackableUnit> OnMoveFailure
                = new Dispatcher<AttackableUnit>();
        public static Dispatcher<AttackableUnit> OnMoveSuccess
                = new Dispatcher<AttackableUnit>();
        public static DataOnlyDispatcher<ObjAIBase, Spell> OnPreAttack
                = new DataOnlyDispatcher<ObjAIBase, Spell>();
        public static DataOnlyDispatcher<AttackableUnit, DamageData> OnPreDealDamage
                = new DataOnlyDispatcher<AttackableUnit, DamageData>();
        public static DataOnlyDispatcher<AttackableUnit, DamageData> OnPreTakeDamage
                = new DataOnlyDispatcher<AttackableUnit, DamageData>();
        public static Dispatcher<ObjAIBase> OnResurrect
                = new Dispatcher<ObjAIBase>();
        public static Dispatcher<Spell> OnSpellCast
                = new Dispatcher<Spell>();
        public static Dispatcher<Spell> OnSpellChannel
                = new Dispatcher<Spell>();
        public static Dispatcher<Spell, ChannelingStopSource> OnSpellChannelCancel
                = new Dispatcher<Spell, ChannelingStopSource>();
        public static Dispatcher<Spell, AttackableUnit, SpellMissile, SpellSector> OnSpellHit
                = new Dispatcher<Spell, AttackableUnit, SpellMissile, SpellSector>();
        public static Dispatcher<SpellMissile> OnSpellMissileEnd
                = new Dispatcher<SpellMissile>();
        public static Dispatcher<SpellMissile, AttackableUnit> OnSpellMissileHit
                = new Dispatcher<SpellMissile, AttackableUnit>();
        public static Dispatcher<SpellMissile, float> OnSpellMissileUpdate
                = new Dispatcher<SpellMissile, float>();
        public static Dispatcher<Spell> OnSpellPostCast
                = new Dispatcher<Spell>();
        public static Dispatcher<Spell> OnSpellPostChannel
                = new Dispatcher<Spell>();
        public static Dispatcher<SpellSector, AttackableUnit> OnSpellSectorHit
                = new Dispatcher<SpellSector, AttackableUnit>();
        public static DataOnlyDispatcher<AttackableUnit, DamageData> OnTakeDamage
                = new DataOnlyDispatcher<AttackableUnit, DamageData>();
        public static DataOnlyDispatcher<ObjAIBase, AttackableUnit> OnTargetLost
                = new DataOnlyDispatcher<ObjAIBase, AttackableUnit>();
        public static Dispatcher<AttackableUnit, Buff> OnUnitBuffDeactivated
                = new Dispatcher<AttackableUnit, Buff>();
        // TODO: Handle crowd control the same as normal dashes.
        public static Dispatcher<AttackableUnit> OnUnitCrowdControlled
                = new Dispatcher<AttackableUnit>();
        // TODO: Change to OnMoveSuccess and change where Publish is called internally to reflect the name.
        public static ConditionDispatcher<ObjAIBase, OrderType> OnUnitUpdateMoveOrder
                = new ConditionDispatcher<ObjAIBase, OrderType>();
        public static Dispatcher<AttackableUnit, float> OnUpdateStats
                = new Dispatcher<AttackableUnit, float>();

        public abstract class DispatcherBase
        {
            public DispatcherBase()
            {
                _dispatchers.Add(this);
            }
            public abstract void RemoveListener(object owner);
        }

        public abstract class DispatcherBase<Source, CBType> : DispatcherBase
        {
            protected class Listener
            {
                public object Owner;
                public Source Source;
                public CBType Callback;
                public bool SingleInstance;
                public Listener(object owner, Source source, CBType callback, bool singleInstance = false)
                {
                    Owner = owner;
                    Source = source;
                    Callback = callback;
                    SingleInstance = singleInstance;
                }
            }
            protected readonly List<Listener> _listeners = new List<Listener>();
            // Storage for Publish functions counters.
            protected List<int> _stack = new List<int>{ -1, -1, -1, -1, -1, -1, -1, -1 };
            // The index of the last Publish function currently executing.
            protected int _nestingLevel = -1;
            protected void IncrementNestingLevel()
            {
                _nestingLevel++;
                if(_nestingLevel >= _stack.Count)
                {
                    _stack.Add(-1);
                }
            }
            // Removes the element and adjusts the counters of all currently executing Publish functions, if necessary.
            protected void CarefulRemoval(int index)
            {
                _listeners.RemoveAt(index);
                for(int l = 0; l < _nestingLevel + 1; l++)
                {
                    if (index < _stack[l])
                    {
                        _stack[l]--;
                    }
                }
            }
            private void CarefulRemoval(Predicate<Listener> match)
            {
                for (int j = _listeners.Count - 1; j >= 0; j--)
                {
                    var listener = _listeners[j];
                    if (match(listener))
                    {
                        CarefulRemoval(j);
                    }
                }
            }
            public void AddListener(object owner, Source source, CBType callback, bool singleInstance = false)
            {
                if (owner == null || source == null || callback == null)
                {
                    return;
                }

                _listeners.Add(
                    new Listener(owner, source, callback, singleInstance)
                );
            }
            public override void RemoveListener(object owner)
            {
                CarefulRemoval(listener => listener.Owner == owner);
            }
            public void RemoveListener(object owner, Source source)
            {
                CarefulRemoval(listener => listener.Owner == owner && listener.Source.Equals(source));
            }
            public void RemoveListener(object owner, Source source, CBType callback)
            {
                CarefulRemoval(listener => listener.Owner == owner && listener.Source.Equals(source) && listener.Callback.Equals(callback));
            }
        }

        public abstract class VariableDispatcherBase<Source, Data, CBType> : VariableDispatcherBase<Source, CBType>
        {
            protected Data _data;
            public void Publish(Source source, Data data)
            {
                _data = data;
                base.Publish(source);
            }
        }

        public abstract class VariableDispatcherBase<Source, CBType> : DispatcherBase<Source, CBType>
        {
            protected Source _source;
            protected abstract void Call(CBType callback);
            protected void Publish(Source source)
            {
                IncrementNestingLevel();
                _source = source;

                int i;
                for (
                    _stack[_nestingLevel] = _listeners.Count - 1;
                    (i = _stack[_nestingLevel]) >= 0;
                    _stack[_nestingLevel]--
                )
                {
                    var listener = _listeners[i];
                    if (listener.Source.Equals(source))
                    {
                        if (listener.SingleInstance)
                        {
                            CarefulRemoval(i);
                        }

                        try
                        {
                            Call(listener.Callback);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e);
                        }
                    }
                }
                _nestingLevel--;
            }
        }

        public abstract class ConditionDispatcherBase<Source, Data, CBType> : DispatcherBase<Source, CBType>
        {
            protected Source _source;
            protected Data _data;
            protected abstract bool Call(CBType callback);
            public bool Publish(Source source, Data data)
            {
                IncrementNestingLevel();
                _source = source;
                _data = data;

                bool returnVal = true;
                int i;
                for (
                    _stack[_nestingLevel] = _listeners.Count - 1;
                    (i = _stack[_nestingLevel]) >= 0;
                    _stack[_nestingLevel]--
                )
                {
                    var listener = _listeners[i];
                    if (listener.Source.Equals(source))
                    {
                        if (listener.SingleInstance)
                        {
                            CarefulRemoval(i);
                        }

                        try
                        {
                            returnVal = returnVal && Call(listener.Callback);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e);
                        }
                    }
                }
                _nestingLevel--;
                return returnVal;
            }
        }

        public class Dispatcher<Source> : VariableDispatcherBase<Source, Action<Source>>
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

        public class Dispatcher<Source, Data> : VariableDispatcherBase<Source, Data, Action<Source, Data>>
        {
            protected override void Call(Action<Source, Data> callback)
            {
                callback(_source, _data);
            }
        }

        public class DataOnlyDispatcher<Source, Data> : VariableDispatcherBase<Source, Data, Action<Data>>
        {
            protected override void Call(Action<Data> callback)
            {
                callback(_data);
            }
        }

        public class Dispatcher<Source, D1, D2, D3> : VariableDispatcherBase<Source, (D1, D2, D3), Action<Source, D1, D2, D3>>
        {
            protected override void Call(Action<Source, D1, D2, D3> callback)
            {
                callback(_source, _data.Item1, _data.Item2, _data.Item3);
            }
        }

        public class Dispatcher<Source, D1, D2, D3, D4> : VariableDispatcherBase<Source, (D1, D2, D3, D4), Action<Source, D1, D2, D3, D4>>
        {
            protected override void Call(Action<Source, D1, D2, D3, D4> callback)
            {
                callback(_source, _data.Item1, _data.Item2, _data.Item3, _data.Item4);
            }
        }

        public class ConditionDispatcher<Source, Data> : ConditionDispatcherBase<Source, Data, Func<Source, Data, bool>>
        {
            protected override bool Call(Func<Source, Data, bool> callback)
            {
                return callback(_source, _data);
            }
        }

        public class ConditionDispatcher<Source, D1, D2> : ConditionDispatcherBase<Source, (D1, D2), Func<Source, D1, D2, bool>>
        {
            protected override bool Call(Func<Source, D1, D2, bool> callback)
            {
                return callback(_source, _data.Item1, _data.Item2);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class ObjAIBase : AttackableUnit
    {
        private Buff[] AppliedBuffs { get; }
        public List<BuffGameScriptController> BuffGameScriptControllers { get; private set; }
        private object _buffsLock = new object();
        private Dictionary<string, Buff> _buffs = new Dictionary<string, Buff>();
        
        public ObjAIBase(string model, Stats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) :
            base(model, stats, collisionRadius, x, y, visionRadius, netId)
        {
            if(!string.IsNullOrEmpty(model))
            {
                AASpellData = _game.Config.ContentManager.GetSpellData(model + "BasicAttack");
                AutoAttackDelay = AASpellData.CastFrame / 30.0f;
                AutoAttackProjectileSpeed = AASpellData.MissileSpeed;
            }
            BuffGameScriptControllers = new List<BuffGameScriptController>();
            AppliedBuffs = new Buff[256];
        }
        
        public BuffGameScriptController AddBuffGameScript(String buffNamespace, String buffClass, Spell ownerSpell, float removeAfter = -1f, bool isUnique = false)
        {
            if (isUnique)
            {
                RemoveBuffGameScriptsWithName(buffNamespace, buffClass);
            }

            BuffGameScriptController buffController = 
                new BuffGameScriptController(this, buffNamespace, buffClass, ownerSpell, duration: removeAfter);
            BuffGameScriptControllers.Add(buffController);
            buffController.ActivateBuff();

            return buffController;
        }
        public void RemoveBuffGameScript(BuffGameScriptController buffController)
        {
            buffController.DeactivateBuff();
            BuffGameScriptControllers.Remove(buffController);
        }
        public bool HasBuffGameScriptActive(String buffNamespace, String buffClass)
        {
            foreach (BuffGameScriptController b in BuffGameScriptControllers)
            {
                if (b.IsBuffSame(buffNamespace, buffClass)) return true;
            }
            return false;
        }
        public void RemoveBuffGameScriptsWithName(String buffNamespace, String buffClass)
        {
            foreach (BuffGameScriptController b in BuffGameScriptControllers)
            {
                if (b.IsBuffSame(buffNamespace, buffClass)) b.DeactivateBuff();
            }
            BuffGameScriptControllers.RemoveAll((b) => b.NeedsRemoved());
        }
        
        public Dictionary<string, Buff> GetBuffs()
        {
            var toReturn = new Dictionary<string, Buff>();
            lock (_buffsLock)
            {
                foreach (var buff in _buffs)
                    toReturn.Add(buff.Key, buff.Value);

                return toReturn;
            }
        }
        
        public int GetBuffsCount()
        {
            return _buffs.Count;
        }
        
        //todo: use statmods
        public Buff GetBuff(string name)
        {
            lock (_buffsLock)
            {
                if (_buffs.ContainsKey(name))
                    return _buffs[name];
                return null;
            }
        }
        
        public void AddBuff(Buff b)
        {
            lock (_buffsLock)
            {
                if (!_buffs.ContainsKey(b.Name))
                {
                    _buffs.Add(b.Name, b);
                }
                else
                {
                    _buffs[b.Name].TimeElapsed = 0; // if buff already exists, just restart its timer
                }
            }
        }

        public void RemoveBuff(Buff b)
        {
            //TODO add every stat
            RemoveBuff(b.Name);
            RemoveBuffSlot(b);
        }

        public void RemoveBuff(string b)
        {
            lock (_buffsLock)
                _buffs.Remove(b);
        }
        
        public byte GetNewBuffSlot(Buff b)
        {
            byte slot = GetBuffSlot();
            AppliedBuffs[slot] = b;
            return slot;
        }

        public void RemoveBuffSlot(Buff b)
        {
            byte slot = GetBuffSlot(b);
            AppliedBuffs[slot] = null;
        }

        private byte GetBuffSlot(Buff buffToLookFor = null)
        {
            for (byte i = 1; i < AppliedBuffs.Length; i++) // Find the first open slot or the slot corresponding to buff
            {
                if (AppliedBuffs[i] == buffToLookFor)
                {
                    return i;
                }
            }
            throw new Exception("No slot found with requested value"); // If no open slot or no corresponding slot
        }

        public override void update(float diff)
        {
            BuffGameScriptControllers.RemoveAll((b) => b.NeedsRemoved());
            base.update(diff);
        }
    }
}

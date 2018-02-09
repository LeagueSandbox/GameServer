using System;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class ObjAIBase : AttackableUnit
    {
        private Buff[] AppliedBuffs { get; }
        private List<BuffGameScriptController> BuffGameScriptControllers { get; }
        private object BuffsLock { get; }
        private Dictionary<string, Buff> Buffs { get; }
        
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
            
            AppliedBuffs = new Buff[256];
            BuffGameScriptControllers = new List<BuffGameScriptController>();
            BuffsLock = new object();
            Buffs = new Dictionary<string, Buff>();

        }
        
        public BuffGameScriptController AddBuffGameScript(string buffNamespace, string buffClass, Spell ownerSpell, float removeAfter = -1f, bool isUnique = false)
        {
            if (isUnique)
            {
                RemoveBuffGameScriptsWithName(buffNamespace, buffClass);
            }

            var buffController = 
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
        public bool HasBuffGameScriptActive(string buffNamespace, string buffClass)
        {
            foreach (var b in BuffGameScriptControllers)
            {
                if (b.IsBuffSame(buffNamespace, buffClass)) return true;
            }
            return false;
        }
        public void RemoveBuffGameScriptsWithName(string buffNamespace, string buffClass)
        {
            foreach (var b in BuffGameScriptControllers)
            {
                if (b.IsBuffSame(buffNamespace, buffClass)) b.DeactivateBuff();
            }
            BuffGameScriptControllers.RemoveAll((b) => b.NeedsRemoved());
        }

        public List<BuffGameScriptController> GetBuffGameScriptController()
        {
            return BuffGameScriptControllers;
        }
        
        public Dictionary<string, Buff> GetBuffs()
        {
            var toReturn = new Dictionary<string, Buff>();
            lock (BuffsLock)
            {
                foreach (var buff in Buffs)
                    toReturn.Add(buff.Key, buff.Value);

                return toReturn;
            }
        }
        
        public int GetBuffsCount()
        {
            return Buffs.Count;
        }
        
        //todo: use statmods
        public Buff GetBuff(string name)
        {
            lock (BuffsLock)
            {
                if (Buffs.ContainsKey(name))
                    return Buffs[name];
                return null;
            }
        }
        
        public void AddBuff(Buff b)
        {
            lock (BuffsLock)
            {
                if (!Buffs.ContainsKey(b.Name))
                {
                    Buffs.Add(b.Name, b);
                }
                else
                {
                    Buffs[b.Name].TimeElapsed = 0; // if buff already exists, just restart its timer
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
            lock (BuffsLock)
                Buffs.Remove(b);
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

using System;
using System.Numerics;
using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    public class Stackable : IStackable
    {
        public int MaxStacks { get; protected set; }
        public int StackCount { get; protected set; }

        public virtual bool IncrementStackCount()
        {
            if (StackCount == MaxStacks)
            {
                return false;
            }
            StackCount++;
            return true;
        }

        public virtual bool DecrementStackCount(int stacksToRemove = 0)
        {
            if (StackCount < 1)
            {
                return false;
            }

            if (stacksToRemove != 0)
            {
                if (stacksToRemove < 0)
                {
                    throw new Exception("Stacks to be Removed can't be a negative number!");
                }

                StackCount = System.Math.Max(StackCount - stacksToRemove, 0);
            }
            else
            {
                StackCount--;
            }

            return true;
        }

        public virtual void SetStacks(int newStacks)
        {
            if (newStacks < 1 || newStacks > MaxStacks)
            {
                throw new System.Exception($"Cannot set stack size out of bounds (max is {MaxStacks})");
            }
            StackCount = newStacks;
        }
    }
}

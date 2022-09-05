﻿using System;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    public class Stackable
    {
        public int MaxStacks { get; protected set; }
        public int StackCount
        {
            get => stackCount; protected set
            {
                if (stackCount != value)
                {
                    OnStackCountChanged(stackCount, value);
                    stackCount = value;
                }
            }
        }

        private int stackCount;

        public virtual bool IncrementStackCount()
        {
            if (StackCount == MaxStacks)
            {
                return false;
            }
            StackCount++;
            return true;
        }

        public virtual bool DecrementStackCount()
        {
            if (StackCount < 1)
            {
                return false;
            }

            StackCount--;
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

        protected virtual void OnStackCountChanged(int oldStackCount, int newStackCount)
        {
        }
    }
}

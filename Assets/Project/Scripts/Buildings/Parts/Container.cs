using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Buildings.Parts
{
    [Serializable]
    public class Container<T>
    {
        [SerializeField] private T containedType;
        [SerializeField] private int containedAmount;
        [SerializeField] private int maxContainableAmount;
        public int ContainedAmount => containedAmount;
        public int MaxContainableAmount=> maxContainableAmount;
        public T ContainedType => containedType;

        public Container()
        {
            containedType = default;
            containedAmount = 0;
            maxContainableAmount = Int32.MaxValue;
        }

        public Container(T type, int containedAmount = 0, int maxContainableAmount = Int32.MaxValue)
        {
            containedType = type;
            this.containedAmount = containedAmount;
            this.maxContainableAmount = maxContainableAmount;
        }

        public bool Add(T item, int amount =1)
        {
            if (!containedType.Equals(item)) return false;
            if (maxContainableAmount - containedAmount <= 0) return false;
            containedAmount+=amount;
            return true;
        }

        public bool AddAmount(int amount = 1)
        {
            if (maxContainableAmount - containedAmount <= 0) return false;
            containedAmount += amount;
            return true;
        }

        public T[] Extract(int amount = 1)
        {
            amount = Mathf.Clamp(amount, 0, containedAmount);
            T[] result = new T[amount];
            for (int i = 0; i < result.Length; i++) result[i] = containedType;
            containedAmount -= amount;
            return result;
        }
    }
}

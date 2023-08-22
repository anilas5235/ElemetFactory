using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Buildings.Parts
{
    public interface IContainable<T>
    {
        public Container<T> GetContainer();
    }

    public class Container<T>
    {
        private Dictionary<T, int> content;

        public bool GetAmount(T contentType, out int amount)
        {
            amount = 0;
            if (!content.ContainsKey(contentType)) return false;
            amount = content[contentType];
            return true;
        }
    }
}

using System;
using System.Linq;
using Project.Scripts.Grid;

namespace Project.Scripts.ItemSystem
{
    public enum ItemForm
    {
        Gas,
        Fluid,
        Solid,
    }

    [Serializable]
    public struct Item
    {
        public int[] resourceIDs;

        public Item(int[] resourceIDs)
        {
            this.resourceIDs = resourceIDs;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType()) return false;
            Item target = (Item) obj;
            if (resourceIDs.Length != target.resourceIDs.Length) return false;
            return !resourceIDs.Where((t, i) => t != target.resourceIDs[i]).Any();
        }

        public override string ToString()
        {
            return resourceIDs.Aggregate(string.Empty, (current, resourceID) => current + (ResourceType)resourceID);
        }
    }
}

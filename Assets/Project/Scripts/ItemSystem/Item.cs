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
        public int[] ResourceIDs;

        public Item(int[] resourceIDs)
        {
            ResourceIDs = resourceIDs;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType()) return false;
            Item target = (Item) obj;
            if (ResourceIDs.Length != target.ResourceIDs.Length) return false;
            return !ResourceIDs.Where((t, i) => t != target.ResourceIDs[i]).Any();
        }

        public override string ToString()
        {
            return ResourceIDs.Aggregate(string.Empty, (current, resourceID) => current + (ResourceType)resourceID);
        }
    }
}

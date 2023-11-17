using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace Project.Scripts.ItemSystem
{
    public enum ResourceType : byte
    {
        None,
        H,
        C,
        O,
        N,
        S,
        Al,
        Fe,
        Na,
        Cl,
    }
    public enum ItemForm : byte
    {
        Gas,
        Fluid,
        Solid,
    }
    public readonly struct Item
    {
        public static readonly Item EmptyItem = new Item();
        public NativeArray<uint> ResourceIDs { get; }
        public ItemForm ItemForm{ get; }
        public float4 Color{ get; }
        public Item(NativeArray<uint> resourceIDs, ItemForm form, float4 color)
        {
            ResourceIDs = new NativeArray<uint>(resourceIDs,Allocator.Persistent);
            ItemForm = form;
            Color = color;
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

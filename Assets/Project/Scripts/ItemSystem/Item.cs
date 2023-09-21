using System;
using System.Linq;
using Project.Scripts.Grid;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

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

    [Serializable]
    public struct Item
    {
        //public int[] ResourceIDs { get; }
        
        public NativeArray<int> ResourceIDs { get; }
        public ItemForm ItemForm{ get; }
        public float4 Color{ get; }
        public Item(NativeArray<int> resourceIDs, ItemForm form, float4 color)
        {
            ResourceIDs = new NativeArray<int>();
            resourceIDs.CopyTo(ResourceIDs);
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

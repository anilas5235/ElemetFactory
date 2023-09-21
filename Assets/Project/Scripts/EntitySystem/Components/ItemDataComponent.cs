using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components
{
    public struct ItemDataComponent : IComponentData
    {
        public uint ItemID;
        public Item item;
        
        public float3 DestinationPos;
        public float3 PreviousPos;
        
        public bool Arrived;
        public float Progress;
    }
}

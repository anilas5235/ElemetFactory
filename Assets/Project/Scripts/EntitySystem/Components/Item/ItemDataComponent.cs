using Project.Scripts.ItemSystem;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components
{
    public struct ItemDataComponent : IComponentData
    {
        public ItemForm itemForm;
        public float4 itemColor;
    }

    public struct ResourceDataPoint : IBufferElementData
    {
        public uint id;
    }
}
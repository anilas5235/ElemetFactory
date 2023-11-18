using Project.Scripts.EntitySystem.Components;
using Project.Scripts.ItemSystem;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ItemDataAspect : IAspect
    {
        public readonly DynamicBuffer<ResourceDataPoint> ResourceDataPoints;
        public readonly RefRO<ItemDataComponent> itemDataComponent;

        public ItemForm ItemForm => itemDataComponent.ValueRO.itemForm;
        public float4 ItemColor => itemDataComponent.ValueRO.itemColor;
    }
}
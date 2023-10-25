using Project.Scripts.EntitySystem.Components;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ItemDataAspect : IAspect
    {
        public readonly DynamicBuffer<ResourceDataPoint> ResourceDataPoints;
        public readonly RefRO<ItemDataComponent> itemDataComponent;
    }
}
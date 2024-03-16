using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Item;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ExtractorAspect : IAspect
    {
        public readonly Entity entity;
        public readonly RefRO<ItemDataComponent> ItemDataComponent;
        public readonly DynamicBuffer<OutputSlot> outputSlots;

        public int ResourceId => ItemDataComponent.ValueRO.ItemID;
    }
}
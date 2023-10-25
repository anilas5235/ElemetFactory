using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.ItemSystem;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ExtractorAspect : IAspect
    {
        public readonly Entity entity;
        public readonly ItemDataAspect ItemDataAspect;
        public readonly DynamicBuffer<OutputSlot> outputSlots;
    }
}
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.ItemSystem;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ExtractorAspect : IAspect
    {
        public readonly Entity entity;

        public readonly RefRW<ExtractorDataComponent> dataComponent;

        private readonly RefRO<LocalTransform> _transform;

        public OutputSlot Output => dataComponent.ValueRW.output;
        public float3 Location => _transform.ValueRO.Position;

        public uint ItemID => dataComponent.ValueRO.itemID;
    }
}

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

        public OutputSlot Output
        {
            get =>dataComponent.ValueRO.output;
            set => dataComponent.ValueRW.output = value;
        }
        public float3 Location => _transform.ValueRO.Position;
    }
}

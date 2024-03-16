using Project.Scripts.EntitySystem.Components.Item;
using Unity.Entities;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ItemEntityAspect : IAspect
    {
        public readonly Entity entity;

        public readonly RefRW<LocalTransform> transform;

        public readonly RefRW<ItemEntityStateDataComponent> dataComponent;
    }
}

using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Unity.Entities;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ItemAspect : IAspect
    {
        public readonly Entity entity;

        public readonly RefRW<LocalTransform> transform;

        public readonly RefRW<ItemDataComponent> dataComponent;

        public readonly RefRW<ItemColor> color;
    }
}

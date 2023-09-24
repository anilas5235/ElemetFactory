using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Buildings
{
    public struct SeparatorDataComponent : IComponentData
    {
        public InputSlot input;
        public NativeArray<OutputSlot> outputs;
    }
}
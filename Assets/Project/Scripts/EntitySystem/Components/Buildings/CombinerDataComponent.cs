using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Buildings
{
    public struct CombinerDataComponent : IComponentData, IHaveInput
    {
        public NativeArray<InputSlot> InputSlots;
        public OutputSlot output;

        public bool TrySetInput(int index, Entity entityToPullFrom)
        {
            return true;
        }
    }
}
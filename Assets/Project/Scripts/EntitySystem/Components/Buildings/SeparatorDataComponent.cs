using Project.Scripts.ItemSystem;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Buildings
{
    public struct SeparatorDataComponent : IComponentData
    {
        public InputSlot input;
        public OutputSlot output1, output2;
    }
}
using Project.Scripts.ItemSystem;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Buildings
{
    public struct TrashCanDataComponent : IComponentData
    {
        public InputSlot input1,input2,input3,input4;
    }
}
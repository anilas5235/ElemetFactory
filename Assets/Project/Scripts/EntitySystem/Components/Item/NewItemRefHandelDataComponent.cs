using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components
{
    public struct NewItemRefHandelDataComponent : IComponentData
    {
        public int SlotNumber;
        public Entity entity;
    }
}
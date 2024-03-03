using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Item
{
    public struct NewItemRefHandelDataComponent : IComponentData
    {
        public int SlotNumber;
        public Entity entity;
    }
}
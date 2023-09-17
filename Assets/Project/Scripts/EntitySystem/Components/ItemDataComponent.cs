using Project.Scripts.ItemSystem;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components
{
    public struct ItemDataComponent : IComponentData
    {
        public Item Item;
        public Entity MySlot;
        public ItemForm MyItemForm;

        public float3 PreviousPos;
        public bool Arrived;
        public float Progress;
    }
}

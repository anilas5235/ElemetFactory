using System;
using Project.Scripts.SlotSystem;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Transmission
{
    [Serializable]
    public struct SlotDataComponent : IComponentData
    {
        public SlotBehaviour MySlotBehaviour { get; }

        public Entity SlotContent;
        public bool IsOccupied => SlotContent != null;
        
        public Entity MyBuilding;

        public SlotDataComponent(SlotBehaviour mySlotBehaviour, Entity slotContent, Entity myBuilding)
        {
            MySlotBehaviour = mySlotBehaviour;
            SlotContent = slotContent;
            MyBuilding = myBuilding;
        }
    }
}

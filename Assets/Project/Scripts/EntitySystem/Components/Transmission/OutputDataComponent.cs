using System;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Transmission
{
    [Serializable]
    public struct OutputDataComponent : IComponentData
    {
        public Entity Slot;
        public Entity SlotToPushTo;

        public OutputDataComponent(Entity outputSlot)
        {
            Slot = outputSlot;
            SlotToPushTo = default;
        }
    }
}
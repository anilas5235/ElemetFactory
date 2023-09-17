using System;
using Unity.Collections;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Transmission
{
    [Serializable]
    public struct InputDataComponent : IComponentData
    {
        public Entity Slot;
        public Entity SlotToPullFrom;

        public InputDataComponent(Entity inputSlot)
        {
            Slot = inputSlot;
            SlotToPullFrom = default;
        }
    }
}

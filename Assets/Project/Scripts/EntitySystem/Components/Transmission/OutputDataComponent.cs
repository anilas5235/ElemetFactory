using System;
using Project.Scripts.SlotSystem;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Transmission
{
    [Serializable]
    [InternalBufferCapacity(3)]
    public struct OutputDataComponent : IBufferElementData
    {
        public float3 Position { get; }

        public SlotBehaviour MySlotBehaviour;

        public Entity SlotContent;
        public bool IsOccupied => SlotContent != default;
        
        public Entity EntityToPushTo;
        public byte OutputIndex { get; }

       
        public OutputDataComponent(float3 position ,SlotBehaviour slotBehaviour , byte outputIndex, Entity slotContent = default)
        {
            EntityToPushTo = default;
            Position = position;
            SlotContent = slotContent;
            MySlotBehaviour = slotBehaviour;
            OutputIndex = outputIndex;
        }
        
        public OutputDataComponent(float3 position ,SlotBehaviour slotBehaviour, Entity entityToPushTo ,byte outputIndex, Entity slotContent = default)
        {
            EntityToPushTo = entityToPushTo;
            OutputIndex = outputIndex;
            Position = position;
            SlotContent = slotContent;
            MySlotBehaviour = slotBehaviour;
        }
    }
}
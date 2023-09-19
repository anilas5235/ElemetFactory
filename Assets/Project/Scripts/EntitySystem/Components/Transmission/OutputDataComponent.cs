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
        public bool IsOccupied => SlotContent != null;
        
        public Entity SlotToPushTo;
        public int BufferIndex;

        public OutputDataComponent(float3 position , Entity slotContent = default)
        {
            SlotToPushTo = default;
            Position = position;
            SlotContent = slotContent;
            MySlotBehaviour = SlotBehaviour.Output;
            BufferIndex = -1;
        }
        public OutputDataComponent(float3 position ,SlotBehaviour slotBehaviour ,Entity slotContent = default)
        {
            SlotToPushTo = default;
            Position = position;
            SlotContent = slotContent;
            MySlotBehaviour = slotBehaviour;
            BufferIndex = -1;
        }
        
        public OutputDataComponent(float3 position ,SlotBehaviour slotBehaviour, Entity slotToPushTo ,int bufferIndex, Entity slotContent = default)
        {
            SlotToPushTo = slotToPushTo;
            Position = position;
            SlotContent = slotContent;
            MySlotBehaviour = slotBehaviour;
            BufferIndex = bufferIndex;
        }
    }
}
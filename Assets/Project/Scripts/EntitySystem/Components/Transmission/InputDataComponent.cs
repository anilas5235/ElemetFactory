using System;
using Project.Scripts.SlotSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Transmission
{
    [Serializable]
    [InternalBufferCapacity(4)]
    public struct InputDataComponent : IBufferElementData
    {
        public float3 Position { get; }

        public SlotBehaviour MySlotBehaviour;

        public Entity SlotContent;
        public bool IsOccupied => SlotContent != null;
        
        public Entity SlotToPullFrom;
        public int BufferIndex;

        public InputDataComponent( float3 position , Entity slotContent = default)
        {
            Position = position;
            SlotToPullFrom = default;
            SlotContent = slotContent;
            MySlotBehaviour = SlotBehaviour.Input;
            BufferIndex = -1;
        }
        public InputDataComponent( float3 position , SlotBehaviour slotBehaviour ,Entity slotContent = default)
        {
            Position = position;
            SlotToPullFrom = default;
            SlotContent = slotContent;
            MySlotBehaviour = slotBehaviour;
            BufferIndex = -1;
        }
        public InputDataComponent( float3 position , SlotBehaviour slotBehaviour, Entity slotToPullFrom, int bufferIndex ,Entity slotContent = default)
        {
            Position = position;
            SlotToPullFrom = slotToPullFrom;
            SlotContent = slotContent;
            MySlotBehaviour = slotBehaviour;
            BufferIndex = bufferIndex;
        }
    }
}

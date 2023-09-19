using System;
using Project.Scripts.SlotSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace Project.Scripts.EntitySystem.Components.Transmission
{
    [Serializable]
    [InternalBufferCapacity(3)]
    public struct InputDataComponent : IBufferElementData
    {
        public float3 Position { get; }

        public SlotBehaviour MySlotBehaviour;

        public Entity SlotContent;
        public bool IsOccupied => SlotContent != default;
        
        public Entity EntityToPullFrom;
        public byte InputIndex { get; }
        
        public InputDataComponent( float3 position , SlotBehaviour slotBehaviour , byte inputIndex, Entity slotContent = default)
        {
            Position = position;
            EntityToPullFrom = default;
            SlotContent = slotContent;
            MySlotBehaviour = slotBehaviour;
            InputIndex = inputIndex;
        }
        public InputDataComponent( float3 position , SlotBehaviour slotBehaviour, Entity entityToPullFrom, byte inputIndex, Entity slotContent = default)
        {
            Position = position;
            EntityToPullFrom = entityToPullFrom;
            InputIndex = inputIndex;
            SlotContent = slotContent;
            MySlotBehaviour = slotBehaviour;
        }
    }
}

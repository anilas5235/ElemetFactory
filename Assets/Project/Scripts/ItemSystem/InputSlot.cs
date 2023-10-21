using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace Project.Scripts.ItemSystem
{
    [Serializable]
    public struct InputSlot : IBufferElementData
    {
        public float3 Position;
        
        public Entity SlotContent;
        public bool IsOccupied => SlotContent != default;
        public bool IsConnected => EntityToPullFrom != default;
        
        public Entity EntityToPullFrom;
        public int outputIndex;
        public InputSlot( float3 position , Entity entityToPullFrom = default, Entity slotContent = default)
        {
            Position = position;
            EntityToPullFrom = entityToPullFrom;
            SlotContent = slotContent;
            outputIndex = 0;
        }
    }
}
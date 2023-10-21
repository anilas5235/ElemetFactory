using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.ItemSystem
{
    [Serializable]
    public struct OutputSlot : IBufferElementData
    {
        public float3 Position;
        
        public Entity SlotContent;
        public bool IsOccupied => SlotContent != default;
        
        public Entity EntityToPushTo;
        public int InputIndex;
        public OutputSlot( float3 position ,  Entity entityToPushTo = default, Entity slotContent = default)
        {
            Position = position;
            EntityToPushTo = entityToPushTo;
            SlotContent = slotContent;
            InputIndex = 0;
        }
    }
}

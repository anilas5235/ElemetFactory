using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.ItemSystem
{
    [Serializable]
    public struct InputSlot
    {
        public float3 Position { get; }
        
        public Entity SlotContent;
        public bool IsOccupied => SlotContent != default;
        
        public Entity EntityToPullFrom;
        public InputSlot( float3 position ,  Entity entityToPullFrom = default, Entity slotContent = default)
        {
            Position = position;
            EntityToPullFrom = entityToPullFrom;
            SlotContent = slotContent;
        }
    }
}
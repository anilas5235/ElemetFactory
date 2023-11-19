using System;
using Project.Scripts.EntitySystem.Aspects;
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
        public int ownIndex;
    }
}
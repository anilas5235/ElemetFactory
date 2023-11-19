using System;
using Project.Scripts.EntitySystem.Aspects;
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

        public bool IsConnected => EntityToPushTo != default;
        
        public Entity EntityToPushTo;
        public int InputIndex;
        public int OwnIndex;
    }
}

using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    public struct CellObject
    {
        public CellObject(int2 position,float3 worldPosition, Entity visuals)
        {
            Position = position;
            Visuals = visuals;
            WorldPosition = worldPosition;
            Building = default;
            Resource = default;
        }
       
        public int2 Position { get; }
        public float3 WorldPosition { get; }
        
        public Entity Building;
        public bool IsOccupied => Building != default;
        public Item Resource;
        public Entity Visuals { get; }
    }
}
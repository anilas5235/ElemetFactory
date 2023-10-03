using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    public struct CellObject
    {
        public CellObject(int2 position,float3 worldPosition, Entity visuals, int2 chunkPosition)
        {
            Position = position;
            Visuals = visuals;
            ChunkPosition = chunkPosition;
            WorldPosition = worldPosition;
            Building = default;
            Resource = default;
        }
       
        public int2 Position { get; }
        public int2 ChunkPosition { get; }
        public float3 WorldPosition { get; }
        
        public Entity Building;
        public bool IsOccupied => Building != default;
        public Item Resource;
        public Entity Visuals { get; }
    }
}
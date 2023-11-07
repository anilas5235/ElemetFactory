using Project.Scripts.ItemSystem;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    public struct CellObject
    {
        public CellObject(int2 position,float3 worldPosition, int2 chunkPosition, Item resource)
        {
            Position = position;
            ChunkPosition = chunkPosition;
            Resource = resource;
            WorldPosition = worldPosition;
            Building = default;
        }
       
        public int2 Position { get; }
        public int2 ChunkPosition { get; }
        public float3 WorldPosition { get; }
        
        public Entity Building;
        public bool IsOccupied => Building != default;
        public Item Resource;

        public bool DeleteBuilding()
        {
            if (Building == default) return false;
            Building = default;
            return true;
        }

        public bool PlaceBuilding(Entity entity)
        {
            if (Building != default) return false;

            Building = entity;

            return true;
        }
    }
}
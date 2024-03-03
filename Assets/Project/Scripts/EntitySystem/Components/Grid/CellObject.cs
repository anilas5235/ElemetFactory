using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    public struct CellObject : IBufferElementData
    {
        public CellObject(int2 position,float3 worldPosition, int2 chunkPosition, int resourceID)
        {
            Position = position;
            ChunkPosition = chunkPosition;
            ResourceID = resourceID;
            WorldPosition = worldPosition;
            Building = default;
        }
       
        public int2 Position { get; }
        public int2 ChunkPosition { get; }
        public float3 WorldPosition { get; }
        
        public Entity Building;
        public bool IsOccupied => Building != default;
        
        public int ResourceID { get; }

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
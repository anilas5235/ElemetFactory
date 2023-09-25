using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    public struct ChunkDataComponent : IComponentData
    {
        public ChunkDataComponent(NativeArray<CellObject> cellObjects, int2 chunkPosition, float3 worldPosition)
        {
            CellObjects = cellObjects;
            ChunkPosition = chunkPosition;
            WorldPosition = worldPosition;
            CellsWithBuilding = new NativeList<int>(Allocator.Persistent);
        }

        public NativeArray<CellObject> CellObjects { get; }
        public NativeList<int> CellsWithBuilding;
        public int2 ChunkPosition { get; }
        public float3 WorldPosition { get; }
    }
}

using Project.Scripts.EntitySystem.Components.Grid;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ChunkDataAspect : IAspect
    {
        private readonly RefRW<ChunkDataComponent> _chunkData;

        public int2 ChunksPosition => _chunkData.ValueRO.ChunkPosition;
        public float3 WorldPosition => _chunkData.ValueRO.WorldPosition;
        
        public static int GetAryIndex(int2 position)
        {
            return position.y * ChunkDataComponent.ChunkSize + position.x;
        }
        public static float3 GetCellWorldPosition(int2 cellPos, float3 chunkWorldPosition)
        {
            return chunkWorldPosition + new float3(cellPos.x + .5f - ChunkDataComponent.HalfChunkSize,
                cellPos.y + .5f - ChunkDataComponent.HalfChunkSize, 0) * ChunkDataComponent.CellSize;
        }
    }
}

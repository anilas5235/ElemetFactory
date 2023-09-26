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
        public int NumPatches => _chunkData.ValueRO.ResourcePatches.Length;

        public bool InView
        {
            get => _chunkData.ValueRO.InView;
            set => _chunkData.ValueRW.InView = value;
        }
        
        public static int GetAryIndex(int2 position)
        {
            return position.y * ChunkDataComponent.ChunkSize + position.x;
        }
        public static float3 GetCellWorldPosition(int2 cellPos, float3 chunkWorldPosition)
        {
            return chunkWorldPosition + new float3(cellPos.x + .5f - ChunkDataComponent.HalfChunkSize,
                cellPos.y + .5f - ChunkDataComponent.HalfChunkSize, 0) * ChunkDataComponent.CellSize;
        }

        public static bool IsValidPositionInChunk(int2 position)
        {
            return position.x >= 0 && position.x < ChunkDataComponent.ChunkSize &&
                   position.y >= 0 && position.y < ChunkDataComponent.ChunkSize;
        }
    }
}

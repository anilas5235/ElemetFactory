using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.EntitySystem.Systems;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ChunkDataAspect : IAspect
    {
        private readonly RefRW<ChunkDataComponent> _chunkData;
        private static int ChunkSize => ChunkDataComponent.ChunkSize;
        public int2 ChunksPosition => _chunkData.ValueRO.ChunkPosition;
        public float3 WorldPosition => _chunkData.ValueRO.WorldPosition;
        public int NumPatches => _chunkData.ValueRO.ResourcePatches.Length;

        public bool InView
        {
            get => _chunkData.ValueRO.InView;
            set => _chunkData.ValueRW.InView = value;
        }

        public CellObject GetCell(int2 position,int2 chunkPosition)
        {
           return IsValidPositionInChunk(position) ?
               _chunkData.ValueRO.CellObjects[GetAryIndex(position)]:
               GetCellFormPseudoPosition(position,chunkPosition);
        }

        private CellObject GetCellFormPseudoPosition(int2 position, int2 chunkPosition)
        {
            int2 chunkOffset = new int2( Mathf.FloorToInt((float)position.x / ChunkSize), Mathf.FloorToInt((float)position.y / ChunkSize));
            int2 newPos = position - chunkOffset * ChunkSize;
            int2 newChunkPos = chunkPosition + chunkOffset;
            GenerationSystem.TryGetChunk(newChunkPos, out ChunkDataAspect chunkData);
            return chunkData.GetCell(newPos,newChunkPos);
        }

        public static int2 GetPseudoPosition(int2 myChunkPosition, int2 otherChunkPosition, int2 position)
        {
            int2 chunkOffset = otherChunkPosition - myChunkPosition;
            return position + chunkOffset * ChunkSize;
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

        public static int2 GetCellPositionFormWorldPosition(float3 worldPosition)
        {
            
        }

        public static bool IsValidPositionInChunk(int2 position)
        {
            return position.x >= 0 && position.x < ChunkDataComponent.ChunkSize &&
                   position.y >= 0 && position.y < ChunkDataComponent.ChunkSize;
        }
    }
}

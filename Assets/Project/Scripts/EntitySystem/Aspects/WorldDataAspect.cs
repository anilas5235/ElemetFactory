using System.Linq;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.EntitySystem.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct WorldDataAspect : IAspect
    {
        private readonly RefRW<WorldDataComponent> worldData;
        public NativeList<PositionChunkPair> ChunkDataBank => worldData.ValueRO.ChunkDataBank;

        public ChunkDataAspect GetChunk(int2 chunkPosition)
        {
            return TryGetValue(chunkPosition, out ChunkDataAspect chunkDataAspect)
                ? chunkDataAspect
                : GenerationSystem.Instance.GenerateChunk(chunkPosition, this);
        }
        public bool TryGetValue(int2 chunkPos, out ChunkDataAspect chunkDataAspect)
        {
            chunkDataAspect = default;
            foreach (var pair in from pair in ChunkDataBank
                     let con = pair.Position == chunkPos
                     where con.x && con.y
                     select pair)
            {
                chunkDataAspect = pair.Chunk;
                return true;
            }

            return false;
        }

        public static float3 GetChunkWorldPosition(int2 chunkPosition)
        {
            float factor = ChunkDataComponent.ChunkSize * ChunkDataComponent.CellSize;
            return new float3((float2)chunkPosition * factor, 0);
        }


        public static int2 GetChunkPosition(float3 transformPosition)
        {
            return new int2(
                Mathf.RoundToInt(transformPosition.x / ChunkDataComponent.ChunkUnitSize),
                Mathf.RoundToInt(transformPosition.y / ChunkDataComponent.ChunkUnitSize));
        }
    }
}

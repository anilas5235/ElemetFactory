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

        public NativeList<PositionChunkPair> ChunkDataBank
        {
            get => worldData.ValueRO.ChunkDataBank;
            set => worldData.ValueRW.ChunkDataBank = value;
        }

        public ChunkDataAspect GetChunk(int2 chunkPosition, out bool newGenerated, EntityCommandBuffer ecb)
        {
            newGenerated = false;
            if (TryGetValue(chunkPosition, out ChunkDataAspect chunkDataAspect)) return chunkDataAspect;

            newGenerated = true;
            PositionChunkPair pair = new PositionChunkPair(GenerationSystem.Instance.GenerateChunk(chunkPosition, this,ecb),
                chunkPosition);
            ChunkDataBank.Add(pair);
            return default;
        }

        public bool TryGetValue(int2 chunkPos, out ChunkDataAspect chunkDataAspect)
        {
            chunkDataAspect = default;
            foreach (var pair in ChunkDataBank)
            {
                if (pair.Position.x != chunkPos.x ||
                    pair.Position.y != chunkPos.y) continue;
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

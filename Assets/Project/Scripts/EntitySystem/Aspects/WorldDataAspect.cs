using System.Linq;
using Project.Scripts.EntitySystem.Buffer;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct WorldDataAspect : IAspect
    {
        private readonly Entity _entity;
        private readonly DynamicBuffer<PositionChunkPair> ChunkDataRefAry;

        public bool TryGetChunk(int2 chunkPos, out ChunkDataAspect chunkDataAspect)
        {
            chunkDataAspect = default;
            
            foreach (var pair in ChunkDataRefAry.AsNativeArray().Where(pair => pair.Position.x == chunkPos.x && pair.Position.y == chunkPos.y))
            {
                chunkDataAspect = pair.Chunk;
                return true;
            }
            return false;
        }

        public bool TryGetPositionChunkPair(int2 chunkPos, out PositionChunkPair pair)
        {
            pair = default;
            
            foreach (var currentPair in ChunkDataRefAry.AsNativeArray().Where(pair => pair.Position.x == chunkPos.x && pair.Position.y == chunkPos.y))
            {
                pair = currentPair;
                return true;
            }
            return false;
        }

        public bool ChunkExits(int2 chunkPos)
        {
            foreach (var pair in ChunkDataRefAry)
            {
                if (pair.Position.x == chunkPos.x && pair.Position.y == chunkPos.y) return true;
            }

            return false;
        }
    }
}
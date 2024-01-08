using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Buffer
{
    public struct PositionChunkPair : IBufferElementData
    {
        public PositionChunkPair(Entity chunkEntity, int2 position, int numOfPatches)
        {
            ChunkEntity = chunkEntity;
            Position = position;
            NumOfPatches = numOfPatches;
        }

        public int2 Position;
        
        public ChunkDataAspect Chunk => GenerationSystem.entityManager.GetAspect<ChunkDataAspect>(ChunkEntity);

        public Entity ChunkEntity;

        public int NumOfPatches;
    }
}
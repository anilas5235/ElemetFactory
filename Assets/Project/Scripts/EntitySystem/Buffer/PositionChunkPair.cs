using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Buffer
{
    public readonly struct PositionChunkPair : IBufferElementData
    {
        public PositionChunkPair(Entity chunkEntity, int2 position)
        {
            ChunkEntity = chunkEntity;
            Position = position;
        }

        public int2 Position { get; }
        
        public ChunkDataAspect Chunk => GenerationSystem.entityManager.GetAspect<ChunkDataAspect>(ChunkEntity);
        
        public Entity ChunkEntity { get; }
    }
}
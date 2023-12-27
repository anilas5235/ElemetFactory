using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Buffer
{
    public readonly struct ChunkGenerationRequestBuffElement : IBufferElementData
    {
        public ChunkGenerationRequestBuffElement(int2 chunkPosition)
        {
            ChunkPosition = chunkPosition;
        }

        public readonly int2 ChunkPosition;
    }
}
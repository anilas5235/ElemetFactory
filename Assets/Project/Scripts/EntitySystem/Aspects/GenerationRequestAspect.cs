using Project.Scripts.EntitySystem.Buffer;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct GenerationRequestAspect : IAspect
    {
        private readonly Entity _entity;
        private readonly DynamicBuffer<ChunkGenerationRequestBuffElement> GenerationRequests;

        public void AddRequest(int2 chunkPosition)
        {
            foreach (var request in GenerationRequests)
            {
                var condition = request.ChunkPosition == chunkPosition;
                if(condition is { x: true, y: true }) return;
            }

            GenerationRequests.Add(new ChunkGenerationRequestBuffElement(chunkPosition));
        }

        public NativeArray<ChunkGenerationRequestBuffElement> GetAllRequests()
        {
            return GenerationRequests.AsNativeArray();
        }

        public void ClearRequestList()
        {
            GenerationRequests.Clear();
        }

        public int GetNumOfRequestCount()
        {
            return GenerationRequests.Length;
        }
    }
}
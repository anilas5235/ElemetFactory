using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    public struct ChunkDataComponent : IComponentData
    {
        public static readonly int ChunkSize = 16;
        public static int CellSize => GenerationSystem.WorldScale;
        public static readonly int ChunkUnitSize = ChunkSize * CellSize;
        public static float HalfChunkSize => ChunkSize/2f;
        public ChunkDataComponent(int2 chunkPosition, float3 worldPosition,
            Entity visualPrefap,ResourcePatch[] resourcePatches, EntityCommandBuffer ecb)
        {
            ChunkPosition = chunkPosition;
            WorldPosition = worldPosition;
            ResourcePatches = new NativeArray<ResourcePatch>(resourcePatches,Allocator.Persistent);
            Buildings = new NativeList<Entity>(0, Allocator.Persistent);

            var obj = new NativeArray<CellObject>(ChunkSize * ChunkSize, Allocator.TempJob);
            for (int y = 0; y < ChunkSize; y++)
            {
                for (int x = 0; x < ChunkSize; x++)
                {
                    int2 pos = new int2(x, y);
                    float3 cellWorldPosition = ChunkDataAspect.GetCellWorldPosition(pos, WorldPosition);
                    Entity entity = ecb.Instantiate(visualPrefap);
                    ecb.SetComponent(entity, new LocalTransform()
                    {
                        Position = cellWorldPosition,
                        Scale = CellSize,
                    });
                    ecb.SetName(entity,$"Tile({pos})ofChunk({chunkPosition})");
                    obj[ChunkDataAspect.GetAryIndex(pos)] = new CellObject(pos, cellWorldPosition, entity,chunkPosition);
                }
            }

            foreach (ResourcePatch resourcePatch in ResourcePatches)
            {
                foreach (int2 position in resourcePatch.Positions)
                {
                    CellObject cellObject = obj[ChunkDataAspect.GetAryIndex(position)];
                    cellObject.Resource = resourcePatch.Resource;
                    obj[ChunkDataAspect.GetAryIndex(position)] = cellObject;
                }
            }

            CellObjects = new NativeArray<CellObject>(obj, Allocator.Persistent);
            obj.Dispose();
            InView = true;
        }

        public NativeArray<CellObject> CellObjects { get; }
        public NativeList<Entity> Buildings;
        public NativeArray<ResourcePatch> ResourcePatches { get; }
        public int2 ChunkPosition { get; }
        public float3 WorldPosition { get; }

        public bool InView;
    }
}

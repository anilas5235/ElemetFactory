using System;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.EntitySystem.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    public struct ChunkDataComponent : IComponentData
    {
        public static int ChunkSize => GenerationSystem.ChunkSize;
        public static int CellSize => GenerationSystem.WorldScale;
        public static int ChunkUnitSize => GenerationSystem.ChunkUnitSize;
        public static int HalfChunkSize => ChunkSize/2;
        public ChunkDataComponent(Entity entity,int2 chunkPosition, float3 worldPosition,
           Entity tilePrefab,NativeArray<ResourcePatch> resourcePatches, EntityCommandBuffer ecb)
        {
            ChunkPosition = chunkPosition;
            WorldPosition = worldPosition;
            ResourcePatches = resourcePatches.Length > 0 ? new NativeArray<ResourcePatch>(resourcePatches,Allocator.Persistent): new NativeArray<ResourcePatch>(0,Allocator.Persistent);

            var cellObjects = new NativeArray<CellObject>(ChunkSize * ChunkSize, Allocator.Temp);
            
            for (var y = 0; y < ChunkSize; y++)
            {
                for (var x = 0; x < ChunkSize; x++)
                {
                    var pos = new int2(x, y);
                    var cellWorldPosition = ChunkDataAspect.GetCellWorldPosition(pos, WorldPosition);
                    cellObjects[ChunkDataAspect.GetAryIndex(pos)] = new CellObject(pos, cellWorldPosition,  chunkPosition, ItemSystem.Item.EmptyItem.ItemID);
                }
            }
            
            foreach (var resourcePatch in ResourcePatches)
            {
                foreach (var position in resourcePatch.Positions)
                {
                    var cellWorldPosition = ChunkDataAspect.GetCellWorldPosition(position, WorldPosition);

                    var itemEntity = ecb.Instantiate(tilePrefab);

                    ecb.SetName(itemEntity, $"Resource");

                    ecb.SetComponent(itemEntity, new LocalTransform()
                    {
                        Position = cellWorldPosition + new float3(0, 0, 1),
                        Scale = CellSize,
                    });
                    cellObjects[ChunkDataAspect.GetAryIndex(position)] = new CellObject(position, cellWorldPosition,chunkPosition,resourcePatch.ItemID);
                }
            }

            var cellBuffer = ecb.AddBuffer<CellObject>(entity);
            cellBuffer.AddRange(cellObjects);
            cellObjects.Dispose();
            ecb.AddBuffer<EntityRefBufferElement>(entity);
            InView = true;
        }
        public NativeArray<ResourcePatch> ResourcePatches { get; }
        public int2 ChunkPosition { get; }
        public float3 WorldPosition { get; }

        public bool InView;
    }
}

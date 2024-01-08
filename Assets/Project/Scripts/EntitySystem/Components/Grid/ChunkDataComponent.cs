using System;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.EntitySystem.Systems;
using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
            PrefabsDataComponent prefabs,NativeArray<ResourcePatch> resourcePatches, EntityCommandBuffer ecb)
        {
            ChunkPosition = chunkPosition;
            WorldPosition = worldPosition;
            ResourcePatches = resourcePatches.Length > 0 ? new NativeArray<ResourcePatch>(resourcePatches,Allocator.Persistent): new NativeArray<ResourcePatch>(0,Allocator.Persistent);

            var cellObjects = new NativeArray<CellObject>(ChunkSize * ChunkSize, Allocator.Temp);
            
            for (int y = 0; y < ChunkSize; y++)
            {
                for (int x = 0; x < ChunkSize; x++)
                {
                    int2 pos = new int2(x, y);
                    float3 cellWorldPosition = ChunkDataAspect.GetCellWorldPosition(pos, WorldPosition);
                    cellObjects[ChunkDataAspect.GetAryIndex(pos)] = new CellObject(pos, cellWorldPosition,  chunkPosition,Item.EmptyItem);
                }
            }
            
            foreach (ResourcePatch resourcePatch in ResourcePatches)
            {
                Item item = resourcePatch.Resource;
                foreach (int2 position in resourcePatch.Positions)
                {
                    float3 cellWorldPosition = ChunkDataAspect.GetCellWorldPosition(position, WorldPosition);
                    
                    var itemEntity = item.ItemForm switch
                    {
                        ItemForm.Gas => ecb.Instantiate(prefabs.GasTile),
                        ItemForm.Fluid => ecb.Instantiate(prefabs.LiquidTile),
                        ItemForm.Solid => ecb.Instantiate(prefabs.SolidTile),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    ecb.AddComponent(itemEntity, new TileColor()
                    {
                        Value = item.Color,
                    });
                    ecb.SetName(itemEntity, $"Resource");


                    ecb.SetComponent(itemEntity, new LocalTransform()
                    {
                        Position = cellWorldPosition + new float3(0, 0, 1),
                        Scale = CellSize,
                    });
                    cellObjects[ChunkDataAspect.GetAryIndex(position)] = new CellObject(position, cellWorldPosition,chunkPosition,item);
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

using System;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.EntitySystem.Systems;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    public struct ChunkDataComponent : IComponentData
    {
        public static readonly int ChunkSize = 16;
        public static int CellSize => GenerationSystem.WorldScale;
        public static readonly int ChunkUnitSize = ChunkSize * CellSize;
        public static int HalfChunkSize => ChunkSize/2;
        public ChunkDataComponent(int2 chunkPosition, float3 worldPosition,
            PrefabsDataComponent prefabs,ResourcePatch[] resourcePatches, EntityCommandBuffer ecb)
        {
            ChunkPosition = chunkPosition;
            WorldPosition = worldPosition;
            ResourcePatches = new NativeArray<ResourcePatch>(resourcePatches,Allocator.Persistent);
            Buildings = new NativeList<Entity>(0, Allocator.Persistent);

            var cellObjects = new NativeArray<CellObject>(ChunkSize * ChunkSize, Allocator.TempJob);
            
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
                    
                    var entity = item.ItemForm switch
                    {
                        ItemForm.Gas => ecb.Instantiate(prefabs.GasTile),
                        ItemForm.Fluid => ecb.Instantiate(prefabs.LiquidTile),
                        ItemForm.Solid => ecb.Instantiate(prefabs.SolidTile),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    ecb.AddComponent(entity, new TileColor()
                    {
                        Value = item.Color,
                    });
                    ecb.SetName(entity, $"Resource");


                    ecb.SetComponent(entity, new LocalTransform()
                    {
                        Position = cellWorldPosition + new float3(0, 0, 1),
                        Scale = CellSize,
                    });
                    cellObjects[ChunkDataAspect.GetAryIndex(position)] = new CellObject(position, cellWorldPosition,chunkPosition,item);
                }
            }
            
            CellObjects = new NativeArray<CellObject>(cellObjects, Allocator.Persistent);
            cellObjects.Dispose();
            InView = true;
        }

        public NativeArray<CellObject> CellObjects;
        public NativeList<Entity> Buildings;
        public NativeArray<ResourcePatch> ResourcePatches { get; }
        public int2 ChunkPosition { get; }
        public float3 WorldPosition { get; }

        public bool InView;
    }
}

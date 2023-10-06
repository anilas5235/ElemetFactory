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
        public static float HalfChunkSize => ChunkSize/2f;
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
                    Item item = Item.EmptyItem;

                    bool itemEmpty = true;
                    foreach (ResourcePatch resourcePatch in ResourcePatches)
                    {
                        foreach (int2 position in resourcePatch.Positions)
                        {
                            if (position.x != pos.x || position.y != pos.y) continue;
                            item = resourcePatch.Resource;
                            itemEmpty = false;
                        }
                    }

                    float3 cellWorldPosition = ChunkDataAspect.GetCellWorldPosition(pos, WorldPosition);
                    Entity entity;

                    if (itemEmpty)
                    {
                       entity= ecb.Instantiate(prefabs.TileVisual);
                       ecb.SetName(entity,$"Tile");
                    }
                    else
                    {
                        entity = item.ItemForm switch
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
                        ecb.SetName(entity,$"Resource");
                    }

                    ecb.SetComponent(entity, new LocalTransform()
                    {
                        Position = cellWorldPosition,
                        Scale = CellSize,
                    });
                    cellObjects[ChunkDataAspect.GetAryIndex(pos)] = new CellObject(pos, cellWorldPosition, entity,chunkPosition,item);
                }
            }
            
            CellObjects = new NativeArray<CellObject>(cellObjects, Allocator.Persistent);
            cellObjects.Dispose();
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

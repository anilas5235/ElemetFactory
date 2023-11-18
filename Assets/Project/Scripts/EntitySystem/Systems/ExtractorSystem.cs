using System;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.ItemSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
    [UpdateAfter(typeof(ChainConveyorSystem))]
    [BurstCompile]
    public partial struct ExtractorSystem : ISystem
    {
        public NativeArray<Entity> prefabsEntities;

        private static bool firstUpdate = true;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrefabsDataComponent>();
            state.RequireForUpdate<ExtractorDataComponent>();
            state.EntityManager.AddComponentData(state.SystemHandle, new TimeRateDataComponent()
            {
                timeSinceLastTick = 0,
                Rate = .25f
            });
        }

        public void OnUpdate(ref SystemState state)
        {
            if (firstUpdate)
            {
                var prefabs = SystemAPI.GetSingleton<PrefabsDataComponent>();
                prefabsEntities = new NativeArray<Entity>(new[]
                {
                    prefabs.ItemGas,
                    prefabs.ItemLiquid,
                    prefabs.ItemSolid
                }, Allocator.Persistent);
                firstUpdate = false;
            }
            var rateHandel = SystemAPI.GetComponent<TimeRateDataComponent>(state.SystemHandle);
            rateHandel.timeSinceLastTick += SystemAPI.Time.DeltaTime;
            if (rateHandel.timeSinceLastTick >= 1f / rateHandel.Rate)
            {
                var ecbSingleton =state.World.GetExistingSystemManaged<EndVariableRateSimulationEntityCommandBufferSystem>();
                
                var ecb = ecbSingleton.CreateCommandBuffer().AsParallelWriter();
                
                var dep = new ExtractorWork()
                {
                    ECB = ecb,
                    WorldScale = GenerationSystem.WorldScale,
                    prefabsEntities = prefabsEntities,
                }.ScheduleParallel(new JobHandle());
                
                ecbSingleton.AddJobHandleForProducer(dep);
                
                rateHandel.timeSinceLastTick = 0;
            }
            
            SystemAPI.SetComponent(state.SystemHandle,rateHandel);
        }
    }
    
    [BurstCompile]
    public partial struct ExtractorWork : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public NativeArray<Entity> prefabsEntities;
        public int WorldScale;

        private void Execute([ChunkIndexInQuery] int index, ExtractorAspect extractorAspect)
        {
            if (extractorAspect.outputSlots[0].IsOccupied) return;

            using NativeArray<ResourceDataPoint> itemResources =
                extractorAspect.ItemDataAspect.ResourceDataPoints.AsNativeArray();

            if (itemResources.Length < 1)
            {
                ECB.RemoveComponent<ExtractorDataComponent>(index, extractorAspect.entity);
                return;
            }

            var itemEntity = CreateItemEntity(index, extractorAspect.outputSlots[0],
                extractorAspect.ItemDataAspect.ResourceDataPoints, extractorAspect.ItemDataAspect.ItemForm,
                extractorAspect.ItemDataAspect.ItemColor, ECB, prefabsEntities, WorldScale);

            ECB.AddComponent(index, extractorAspect.entity, new NewItemRefHandelDataComponent()
            {
                entity = itemEntity,
                SlotNumber = 0,
            });
        }

        public static Entity CreateItemEntity(int index, OutputSlot outputSlot, DynamicBuffer<ResourceDataPoint> resourceDataPoints,
          ItemForm itemForm, float4 itemColor  ,EntityCommandBuffer.ParallelWriter ECB, NativeArray<Entity> prefabs, int worldScale)
        {

            var itemEntity = itemForm switch
            {
                ItemForm.Gas => ECB.Instantiate(index, prefabs[0]),
                ItemForm.Fluid => ECB.Instantiate(index, prefabs[1]),
                ItemForm.Solid => ECB.Instantiate(index, prefabs[2]),
                _ => throw new ArgumentOutOfRangeException()
            };

            ECB.SetComponent(index, itemEntity, new ItemEntityStateDataComponent()
            {
                Arrived = true,
                DestinationPos = outputSlot.Position,
                PreviousPos = outputSlot.Position,
            });


            DynamicBuffer<ResourceDataPoint> bufferResource = ECB.SetBuffer<ResourceDataPoint>(index, itemEntity);

            bufferResource.AddRange(resourceDataPoints.AsNativeArray());

            ECB.SetComponent(index, itemEntity, new LocalTransform()
            {
                Position = outputSlot.Position,
                Scale = worldScale * .7f,
            });

            ECB.SetComponent(index, itemEntity, new ItemColor()
            {
                Value = itemColor,
            });

            ECB.SetComponent(index, itemEntity, new ItemDataComponent()
            {
                itemForm = itemForm,
                itemColor = itemColor,
            });

            ECB.SetName(index, itemEntity, "Item");
            return itemEntity;
        }
    }
}

using System;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
    [UpdateAfter(typeof(ChainConveyorSystem))]
    [BurstCompile]
    public partial struct ExtractorSystem : ISystem
    {
        private NativeArray<Entity> prefabsEntities;
        
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

                using var prefabs = new NativeArray<Entity>(prefabsEntities,Allocator.TempJob);
                
                var dep = new ExtractorWork()
                {
                    ECB = ecb,
                    WorldScale = GenerationSystem.WorldScale,
                    prefabsEntities = prefabs,
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
        [NativeDisableContainerSafetyRestriction] public NativeArray<Entity> prefabsEntities;
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
                itemResources, extractorAspect.ItemDataAspect.ItemForm,
                extractorAspect.ItemDataAspect.ItemColor, ECB, prefabsEntities, WorldScale);

            ECB.AddComponent(index, extractorAspect.entity, new NewItemRefHandelDataComponent()
            {
                entity = itemEntity,
                SlotNumber = 0,
            });
        }

        public static Entity CreateItemEntity(int index, OutputSlot outputSlot,
            NativeArray<ResourceDataPoint> resourceDataPointsA, NativeArray<ResourceDataPoint> resourceDataPointsB
            , EntityCommandBuffer.ParallelWriter ECB, NativeArray<Entity> prefabs,
            int worldScale, NativeArray<ResourceLookUpData> resourceLookUpData)
        {
            using var itemBuffer = new NativeList<ResourceDataPoint>(Allocator.TempJob);
            itemBuffer.AddRange(resourceDataPointsA);
            itemBuffer.AddRange(resourceDataPointsB);


            float4 color = float4.zero;
            float form = 0;
            foreach (var id in itemBuffer)
            {
                ResourceLookUpData lookUpData = resourceLookUpData[(int)id.id];
                color += new float4(lookUpData.color.r, lookUpData.color.g, lookUpData.color.b, 0);
                form += (int)lookUpData.form;
            }

            color *= 1f / itemBuffer.Length;
            color.w = 1f;
            form *= 1f / itemBuffer.Length;
            form = Mathf.RoundToInt(form);

            return CreateItemEntity(index, outputSlot, itemBuffer, (ItemForm)form,
                color, ECB, prefabs, worldScale);
        }


        public static Entity CreateItemEntity(int index, OutputSlot outputSlot, NativeArray<ResourceDataPoint> resourceDataPoints,
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
          
            bufferResource.AddRange(resourceDataPoints);

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

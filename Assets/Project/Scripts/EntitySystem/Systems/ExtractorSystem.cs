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
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
    [UpdateAfter(typeof(ChainConveyorSystem))]
    [BurstCompile]
    public partial class ExtractorSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<PrefabsDataComponent>();
            RequireForUpdate<ExtractorDataComponent>();
            EntityManager.AddComponentData(SystemHandle, new TimeRateDataComponent()
            {
                timeSinceLastTick = 0,
                Rate = .25f
            });
        }
        protected override void OnUpdate()
        {
            var rateHandel = SystemAPI.GetComponent<TimeRateDataComponent>(SystemHandle);
            rateHandel.timeSinceLastTick += SystemAPI.Time.DeltaTime;
            if (rateHandel.timeSinceLastTick >= 1f / rateHandel.Rate)
            {
                var ecbSingleton = World.GetExistingSystemManaged<EndVariableRateSimulationEntityCommandBufferSystem>();
                
                var prefabs = SystemAPI.GetSingleton<PrefabsDataComponent>();

                var ecb = ecbSingleton.CreateCommandBuffer().AsParallelWriter();
                
                var dep = new ExtractorWork()
                {
                    ECB = ecb,
                    GasItemPrefab = prefabs.ItemGas,
                    FluidItemPrefab = prefabs.ItemLiquid,
                    SolidItemPrefab = prefabs.ItemSolid,
                    WorldScale = GenerationSystem.WorldScale,
                }.ScheduleParallel(new JobHandle());
                
                ecbSingleton.AddJobHandleForProducer(dep);
                
                rateHandel.timeSinceLastTick = 0;
            }
            
            SystemAPI.SetComponent(SystemHandle,rateHandel);
        }
    }
    
    [BurstCompile]
    public partial struct ExtractorWork : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public Entity GasItemPrefab;
        public Entity FluidItemPrefab;
        public Entity SolidItemPrefab;
        public int WorldScale;

        private void Execute([ChunkIndexInQuery]int index,ExtractorAspect extractorAspect)
        {
            if(extractorAspect.outputSlots[0].IsOccupied) return;

            using NativeArray<ResourceDataPoint> itemResources = extractorAspect.ItemDataAspect.ResourceDataPoints.AsNativeArray();

            if (itemResources.Length < 1)
            {
                ECB.RemoveComponent<ExtractorDataComponent>(index,extractorAspect.entity);
                return;
            }

            var itemEntity = extractorAspect.ItemDataAspect.itemDataComponent.ValueRO.itemForm switch
            {
                ItemForm.Gas => ECB.Instantiate(index,GasItemPrefab),
                ItemForm.Fluid => ECB.Instantiate(index,FluidItemPrefab),
                ItemForm.Solid => ECB.Instantiate(index,SolidItemPrefab),
                _ => throw new ArgumentOutOfRangeException()
            };

            ECB.SetComponent(index,itemEntity, new ItemEntityStateDataComponent()
            {
                Arrived = true,
                DestinationPos = extractorAspect.outputSlots[0].Position,
                PreviousPos = extractorAspect.outputSlots[0].Position,
            });

           
            DynamicBuffer<ResourceDataPoint> bufferResource = ECB.SetBuffer<ResourceDataPoint>(index,itemEntity);

            foreach (ResourceDataPoint itemResource in itemResources)
            {
                bufferResource.Add(itemResource);
            }
            
            ECB.SetComponent(index,itemEntity, new LocalTransform()
            {
                Position = extractorAspect.outputSlots[0].Position,
                Scale = WorldScale*.7f,
            });
            
            ECB.SetComponent(index,itemEntity, new ItemColor()
            {
                Value = extractorAspect.ItemDataAspect.itemDataComponent.ValueRO.itemColor,
            });
            
            ECB.SetComponent(index,itemEntity, new ItemDataComponent()
            {
                itemForm = extractorAspect.ItemDataAspect.itemDataComponent.ValueRO.itemForm,
                itemColor = extractorAspect.ItemDataAspect.itemDataComponent.ValueRO.itemColor,
            });
            
            ECB.SetName(index,itemEntity,"Item");
            
            ECB.AddComponent(index,extractorAspect.entity,new NewItemRefHandelDataComponent()
            {
                entity = itemEntity,
                SlotNumber = 0,
            });
        }
    }
}

using System;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Systems
{
    [BurstCompile]
    public partial struct ExtractorSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrefabsDataComponent>();
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<ExtractorDataComponent>();
            state.EntityManager.AddComponentData(state.SystemHandle, new TimeRateDataComponent()
            {
                timeSinceLastTick = 0,
                Rate = .25f
            });
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var rateHandel = SystemAPI.GetComponent<TimeRateDataComponent>(state.SystemHandle);
            rateHandel.timeSinceLastTick += SystemAPI.Time.DeltaTime;
            if (rateHandel.timeSinceLastTick >= 1f / rateHandel.Rate)
            {
                var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();

                var prefabs = SystemAPI.GetSingleton<PrefabsDataComponent>();

                new ExtractorWork()
                {
                    ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged),
                    GasItemPrefab = prefabs.ItemGas,
                    FluidItemPrefab = prefabs.ItemLiquid,
                    SolidItemPrefab = prefabs.ItemSolid,
                    WorldScale = GenerationSystem.WorldScale,
                }.Schedule();
                
                rateHandel.timeSinceLastTick = 0;
            }
            
            SystemAPI.SetComponent(state.SystemHandle,rateHandel);
        }
    }
    
    [BurstCompile]
    public partial struct ExtractorWork : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public Entity GasItemPrefab;
        public Entity FluidItemPrefab;
        public Entity SolidItemPrefab;
        public int WorldScale;

        private void Execute(ExtractorAspect extractorAspect)
        {
            if(extractorAspect.outputSlots[0].IsOccupied) return;

            using NativeArray<ResourceDataPoint> itemResources = extractorAspect.ItemDataAspect.ResourceDataPoints.AsNativeArray();
            
            if(itemResources.Length < 1) return;

            var itemEntity = extractorAspect.ItemDataAspect.itemDataComponent.ValueRO.itemForm switch
            {
                ItemForm.Gas => ECB.Instantiate(GasItemPrefab),
                ItemForm.Fluid => ECB.Instantiate(FluidItemPrefab),
                ItemForm.Solid => ECB.Instantiate(SolidItemPrefab),
                _ => throw new ArgumentOutOfRangeException()
            };

            ECB.SetComponent(itemEntity, new ItemEntityStateDataComponent()
            {
                Arrived = true,
                DestinationPos = extractorAspect.outputSlots[0].Position,
            });

            DynamicBuffer<ResourceDataPoint> bufferResource = ECB.SetBuffer<ResourceDataPoint>(itemEntity);

            foreach (ResourceDataPoint itemResource in itemResources)
            {
                bufferResource.Add(itemResource);
            }
            
            ECB.SetComponent(itemEntity, new LocalTransform()
            {
                Position = extractorAspect.outputSlots[0].Position,
                Scale = WorldScale,
            });
        }
    }
}

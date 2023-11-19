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
        private static EndVariableRateSimulationEntityCommandBufferSystem _endVariableECBSys;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrefabsDataComponent>();
            state.RequireForUpdate<ExtractorDataComponent>();
            _endVariableECBSys =
                state.World.GetOrCreateSystemManaged<EndVariableRateSimulationEntityCommandBufferSystem>();
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
                var ecb = _endVariableECBSys.CreateCommandBuffer().AsParallelWriter();

                using var prefabs = new NativeArray<Entity>(prefabsEntities,Allocator.TempJob);
                
                var dep = new ExtractorWork()
                {
                    ECB = ecb,
                    WorldScale = GenerationSystem.WorldScale,
                    prefabsEntities = prefabs,
                }.ScheduleParallel(new JobHandle());
                
                _endVariableECBSys.AddJobHandleForProducer(dep);
                
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

            ItemEntityUtility.CreateItemEntity(index,extractorAspect.entity ,extractorAspect.outputSlots[0],
                itemResources, extractorAspect.ItemDataAspect.ItemForm,
                extractorAspect.ItemDataAspect.ItemColor, ECB, prefabsEntities, WorldScale);
        }
    }
}

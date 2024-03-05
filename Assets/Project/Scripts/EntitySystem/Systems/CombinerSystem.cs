using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.DataObject;
using Project.Scripts.EntitySystem.Components.Item;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
    [UpdateAfter(typeof(ChainConveyorSystem))]
    [BurstCompile]
    public partial struct CombinerSystem : ISystem
    {
        private static EndVariableRateSimulationEntityCommandBufferSystem _endVariableECBSys;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ItemPrefabsDataComponent>();
            state.RequireForUpdate<CombinerDataComponent>();
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
            var rateHandel = SystemAPI.GetComponent<TimeRateDataComponent>(state.SystemHandle);
            rateHandel.timeSinceLastTick += SystemAPI.Time.DeltaTime;
            if (rateHandel.timeSinceLastTick >= 1f / rateHandel.Rate)
            {
                var ecb = _endVariableECBSys.CreateCommandBuffer().AsParallelWriter();
                
                var dep = new CombinerWork()
                {
                    ECB = ecb,
                    WorldScale = GenerationSystem.WorldScale,
                    ItemDataLookup = SystemAPI.GetComponentLookup<ItemDataComponent>(),
                }.ScheduleParallel(new JobHandle());
                
                _endVariableECBSys.AddJobHandleForProducer(dep);
                
                rateHandel.timeSinceLastTick = 0;
            }
            
            SystemAPI.SetComponent(state.SystemHandle,rateHandel);
        }
    }

    [BurstCompile]
    public partial struct CombinerWork : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        [NativeDisableContainerSafetyRestriction]
        public ComponentLookup<ItemDataComponent> ItemDataLookup;

        public int WorldScale;

        private void Execute([ChunkIndexInQuery] int index, BuildingAspect buildingAspect,
            CombinerDataComponent combinerDataComponent)
        {
            if (buildingAspect.outputSlots[0].IsOccupied) return;

            if (!buildingAspect.inputSlots[0].IsOccupied || !buildingAspect.inputSlots[1].IsOccupied) return;
            
            //TODO: New Recipe system
               
            ECB.DestroyEntity(index,buildingAspect.inputSlots[0].SlotContent);
            ECB.DestroyEntity(index,buildingAspect.inputSlots[1].SlotContent);
            buildingAspect.inputSlots.ElementAt(0).SlotContent = default;
            buildingAspect.inputSlots.ElementAt(1).SlotContent = default;
        }
    }
}

using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
    [UpdateAfter(typeof(ChainConveyorSystem))]
    [BurstCompile]
    public partial struct TrashCanSystem : ISystem
    {
        private static EndVariableRateSimulationEntityCommandBufferSystem _endVariableECBSys;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrefabsDataComponent>();
            state.RequireForUpdate<TrashCanDataComponent>();
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
                
                var dep = new TrashCanWork()
                {
                    ECB = ecb,
                }.ScheduleParallel(new JobHandle());
                
                _endVariableECBSys.AddJobHandleForProducer(dep);
                
                rateHandel.timeSinceLastTick = 0;
            }
            
            SystemAPI.SetComponent(state.SystemHandle,rateHandel);
        }
    }

    [BurstCompile]
    public partial struct TrashCanWork : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute([ChunkIndexInQuery] int index, BuildingAspect buildingAspect,
            TrashCanDataComponent trashCanDataComponent)
        {
            for (int i = 0; i < buildingAspect.inputSlots.Length; i++)
            {
                var slot = buildingAspect.inputSlots[i];
                if(!slot.IsOccupied) continue;
                ECB.DestroyEntity(index,slot.SlotContent);
                buildingAspect.inputSlots.ElementAt(i).SlotContent = default;
            }
        }
    }
}

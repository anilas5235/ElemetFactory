using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.Utilities;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
    [UpdateAfter(typeof(ChainConveyorSystem))]
    [BurstCompile]
    public partial struct ExtractorSystem : ISystem
    {
        private static EndVariableRateSimulationEntityCommandBufferSystem _endVariableECBSys;
        public void OnCreate(ref SystemState state)
        {
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
            var rateHandel = SystemAPI.GetComponent<TimeRateDataComponent>(state.SystemHandle);
            rateHandel.timeSinceLastTick += SystemAPI.Time.DeltaTime;
            if (rateHandel.timeSinceLastTick >= 1f / rateHandel.Rate)
            {
                var ecb = _endVariableECBSys.CreateCommandBuffer().AsParallelWriter();
                
                var dep = new ExtractorWork()
                {
                    ECB = ecb,
                    WorldScale = GenerationSystem.WorldScale,
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
        public int WorldScale;

        private void Execute([ChunkIndexInQuery] int index, ExtractorAspect extractorAspect)
        {
            if (extractorAspect.outputSlots[0].IsOccupied) return;

            if (extractorAspect.ResourceId < 0)
            {
                ECB.RemoveComponent<ExtractorDataComponent>(index, extractorAspect.entity);
                return;
            }

            ItemEntityUtility.CreateItemEntity(index,extractorAspect.entity ,extractorAspect.outputSlots[0],
                extractorAspect.ResourceId, ECB, WorldScale);
        }
    }
}

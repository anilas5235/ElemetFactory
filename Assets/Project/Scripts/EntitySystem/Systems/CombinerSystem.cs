using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
    [UpdateAfter(typeof(ChainConveyorSystem))]
    [BurstCompile]
    public partial struct CombinerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrefabsDataComponent>();
            state.RequireForUpdate<CombinerDataComponent>();
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
                var ecbSingleton =state.World.GetExistingSystemManaged<EndVariableRateSimulationEntityCommandBufferSystem>();
                
                var prefabs = SystemAPI.GetSingleton<PrefabsDataComponent>();

                var ecb = ecbSingleton.CreateCommandBuffer().AsParallelWriter();
                
                var dep = new CombinerWork()
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
            
            SystemAPI.SetComponent(state.SystemHandle,rateHandel);
        }
    }

    [BurstCompile]
    public partial struct CombinerWork : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public Entity GasItemPrefab;
        public Entity FluidItemPrefab;
        public Entity SolidItemPrefab;
        public int WorldScale;

        private void Execute([ChunkIndexInQuery] int index, BuildingAspect extractorAspect,CombinerDataComponent combinerDataComponent)
        {
            
        }
    }
}

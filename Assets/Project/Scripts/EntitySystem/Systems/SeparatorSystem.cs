using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Item;
using Project.Scripts.Utilities;
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
    public partial struct SeparatorSystem : ISystem
    {
        private NativeArray<Entity> prefabsEntities;
        
        private static bool firstUpdate = true;
        private static EndVariableRateSimulationEntityCommandBufferSystem _endVariableECBSys;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrefabsDataComponent>();
            state.RequireForUpdate<SeparatorDataComponent>();
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
                
                var dep = new SeparatorWork()
                {
                    ECB = ecb,
                    WorldScale = GenerationSystem.WorldScale,
                    prefabsEntities = prefabs,
                    resourceBufferLookup = SystemAPI.GetComponentLookup<ItemDataComponent>()
                }.ScheduleParallel(new JobHandle());
                
                _endVariableECBSys.AddJobHandleForProducer(dep);
                
                rateHandel.timeSinceLastTick = 0;
            }
            
            SystemAPI.SetComponent(state.SystemHandle,rateHandel);
        }
    }
    
    [BurstCompile]
    public partial struct SeparatorWork : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Entity> prefabsEntities;

        [NativeDisableContainerSafetyRestriction]
        public ComponentLookup<ItemDataComponent> resourceBufferLookup;

        public int WorldScale;

        private void Execute([ChunkIndexInQuery] int index, BuildingAspect buildingAspect,
            SeparatorDataComponent separatorDataComponent)
        {
            if (buildingAspect.outputSlots[0].IsOccupied || buildingAspect.outputSlots[1].IsOccupied) return;

            if (!buildingAspect.inputSlots[0].IsOccupied) return;

            //TODO: New Recipe system
            
            ECB.DestroyEntity(index, buildingAspect.inputSlots[0].SlotContent);
            buildingAspect.inputSlots.ElementAt(0).SlotContent = default;
        }
    }
}

using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Flags;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial struct NewChunkDetectorSystem : ISystem
    {
        private static BeginSimulationEntityCommandBufferSystem _ecbSingleton;
        public void OnCreate(ref SystemState state)
        {
           state.RequireForUpdate<NewChunkDataComponent>();
           _ecbSingleton = state.World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var ecb = _ecbSingleton.CreateCommandBuffer();
            var jobHandle = new NewChunkHandle()
            {
                worldData = SystemAPI.GetBuffer<PositionChunkPair>(GenerationSystem.worldDataEntity),
                ECB = ecb,
            }.Schedule(new JobHandle());
            
            _ecbSingleton.AddJobHandleForProducer(jobHandle);
        }
    }
    
    [BurstCompile]
    public partial struct NewChunkHandle : IJobEntity
    {
        public EntityCommandBuffer ECB;
        [NativeDisableContainerSafetyRestriction] public DynamicBuffer<PositionChunkPair> worldData;
        
        private void Execute(Entity entity, in NewChunkDataComponent flag)
        {
            worldData.Add(new PositionChunkPair(entity, flag.Position,flag.PatchNum));
            ECB.RemoveComponent<NewChunkDataComponent>(entity);
        }
    }
}
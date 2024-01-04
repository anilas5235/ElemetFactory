using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Flags;
using Project.Scripts.EntitySystem.Components.Grid;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    public partial struct NewChunkDetectorSystem : ISystem
    {
        private static BeginFixedStepSimulationEntityCommandBufferSystem ecbSingleton;
        
        
        public void OnCreate(ref SystemState state)
        {
           state.RequireForUpdate<NewChunkDataComponent>();
           ecbSingleton = state.World.GetOrCreateSystemManaged<BeginFixedStepSimulationEntityCommandBufferSystem>();
        }

        
        public void OnUpdate(ref SystemState state)
        {
            var ecb = ecbSingleton.CreateCommandBuffer();
            var jobHandle = new NewChunkHandle()
            {
                worldDataAspect = SystemAPI.GetAspect<WorldDataAspect>(GenerationSystem.worldDataEntity),
                ECB = ecb,
            }.Schedule(new JobHandle());
            
            ecbSingleton.AddJobHandleForProducer(jobHandle);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
    
    [BurstCompile]
    public partial struct NewChunkHandle : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public WorldDataAspect worldDataAspect;
        
        private void Execute(Entity entity, NewChunkDataComponent flag, ChunkDataComponent chunkDataComponent)
        {
            worldDataAspect.AddChunksToData(new PositionChunkPair(entity, chunkDataComponent.ChunkPosition));
            ECB.RemoveComponent<NewChunkDataComponent>(entity);
        }
    }
}
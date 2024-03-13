using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Item;
using Project.Scripts.ItemSystem;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace Project.Scripts.EntitySystem.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class NewItemHandelSystem : SystemBase
    {
        private static EndSimulationEntityCommandBufferSystem ecbSingleton;
        protected override void OnCreate()
        {
            RequireForUpdate<NewItemRefHandelDataComponent>();
            ecbSingleton = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = ecbSingleton.CreateCommandBuffer().AsParallelWriter();
            var dep = new NewItemHandel()
            {
                ECB = ecb,
                outputsLookup = SystemAPI.GetBufferLookup<OutputSlot>()
            }.ScheduleParallel(new JobHandle());
            
            ecbSingleton.AddJobHandleForProducer(dep);
        }
    }

    [BurstCompile]
    public partial struct NewItemHandel : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        [NativeDisableContainerSafetyRestriction] public BufferLookup<OutputSlot> outputsLookup;
        
        private void Execute(Entity entity,[ChunkIndexInQuery] int index, NewItemRefHandelDataComponent itemRefHandelDataComponent)
        {
            var buffer = outputsLookup[entity];
            buffer.ElementAt(itemRefHandelDataComponent.SlotNumber).SetSlotContent(itemRefHandelDataComponent.entity);
            ECB.RemoveComponent<NewItemRefHandelDataComponent>(index,entity);
        }
    }
}
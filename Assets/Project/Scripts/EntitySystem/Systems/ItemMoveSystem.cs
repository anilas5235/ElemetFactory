using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial struct ItemMoveSystem : ISystem
    {
        private SystemHandle chainSystemHandle;
        private static bool firstUpdate = true;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ItemDataComponent>();
            state.RequireForUpdate<ChainConveyorSystem>();
        }
      
        public void OnUpdate(ref SystemState state)
        {
            if (firstUpdate)
            {
                chainSystemHandle = state.World.GetExistingSystem<ChainConveyorSystem>();
                firstUpdate = false;
            }

            new ItemMoveJob()
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                conveyorSpeed = SystemAPI.GetComponent<TimeRateDataComponent>(chainSystemHandle).Rate, 
            }.ScheduleParallel();
        }
    }
    
    [BurstCompile]
    public partial struct ItemMoveJob : IJobEntity
    {
        public float deltaTime;
        public float conveyorSpeed;

        private void Execute(ItemEntityAspect itemEntityAspect)
        {
            if (itemEntityAspect.dataComponent.ValueRO.Arrived) return;

            itemEntityAspect.dataComponent.ValueRW.Progress += deltaTime * conveyorSpeed;

            if (itemEntityAspect.dataComponent.ValueRO.Progress > .98f)
            {
                itemEntityAspect.dataComponent.ValueRW.Arrived = true;
                itemEntityAspect.transform.ValueRW.Position = itemEntityAspect.dataComponent.ValueRO.DestinationPos;
                return;
            }

            itemEntityAspect.transform.ValueRW.Position =
                math.lerp(itemEntityAspect.dataComponent.ValueRO.PreviousPos,
                    itemEntityAspect.dataComponent.ValueRO.DestinationPos,
                    itemEntityAspect.dataComponent.ValueRO.Progress);
        }
    }
}

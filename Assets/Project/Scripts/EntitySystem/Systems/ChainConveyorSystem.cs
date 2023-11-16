using System.Collections.Generic;
using System.Linq;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Tags;
using Project.Scripts.ItemSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(VariableRateSimulationSystemGroup),OrderFirst = true)]
    public partial struct ChainConveyorSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<DynamicBuffer<ConveyorChainDataPoint>>();
            state.EntityManager.AddComponentData(state.SystemHandle, new TimeRateDataComponent()
            {
                timeSinceLastTick = 0,
                Rate = 1f
            });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var rateHandel = SystemAPI.GetComponent<TimeRateDataComponent>(state.SystemHandle);
            rateHandel.timeSinceLastTick += SystemAPI.Time.DeltaTime;
            if (rateHandel.timeSinceLastTick >= 1f / rateHandel.Rate)
            {
                var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

                using var chainJobs = new NativeList<ChainJob>(Allocator.Temp);
                EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                
                var dependency = new ChainJob().ScheduleParallel(new JobHandle());

                var dependency2 = new PushPullJob()
                {
                    entityManager = state.EntityManager,
                }.ScheduleParallel(dependency);
                
                rateHandel.timeSinceLastTick = 0;
            }

            SystemAPI.SetComponent(state.SystemHandle, rateHandel);
        }
    }

    [BurstCompile]
    public partial struct ChainJob : IJobEntity
    {
        //[NativeDisableContainerSafetyRestriction]
        private void Execute(ConveyorChainDataAspect conveyorChainDataAspect)
        {
            var chain = conveyorChainDataAspect.ConveyorChainData;

            for (var index = 0; index < chain.Length; index++)
            {
                var currentBuilding = chain[index].ConveyorAspect;

                if (index + 1 < chain.Length)
                {
                    var nextBuilding = chain[index + 1].ConveyorAspect;

                    //push out to next Building
                    if (currentBuilding.outputSlots[0].IsOccupied &&
                        !nextBuilding.inputSlots[0].IsOccupied)
                    {
                        nextBuilding.inputSlots.ElementAt(0).SlotContent =
                            currentBuilding.outputSlots[0].SlotContent;
                        currentBuilding.outputSlots.ElementAt(0).SlotContent = Entity.Null;
                    }
                }

                //push input to output
                if (!currentBuilding.outputSlots[0].IsOccupied && currentBuilding.inputSlots[0].IsOccupied)
                {
                    currentBuilding.outputSlots.ElementAt(0).SlotContent =
                        currentBuilding.inputSlots[0].SlotContent;
                    currentBuilding.inputSlots.ElementAt(0).SlotContent = Entity.Null;
                }
            }
        }
    }


    [BurstCompile]
    public partial struct PushPullJob: IJobEntity
    {
        public EntityManager entityManager;
        private void Execute(BuildingAspect buildingAspect,BuildingWithPushPullTag tag)
        {
            for (var inputIndex = 0; inputIndex < buildingAspect.inputSlots.Length; inputIndex++)
            {
                var inputSlot = buildingAspect.inputSlots[inputIndex];
                var currentBuilding = buildingAspect;
                if (!inputSlot.IsConnected || inputSlot.IsOccupied) continue;

                var nextBuilding = entityManager.GetAspect<BuildingAspect>(inputSlot.EntityToPullFrom);

                //push out to next Building
                if (!nextBuilding.outputSlots[inputSlot.outputIndex].IsOccupied)
                {
                    currentBuilding.inputSlots.ElementAt(inputIndex).SlotContent =
                        nextBuilding.outputSlots[inputSlot.outputIndex].SlotContent;
                    nextBuilding.outputSlots.ElementAt(inputSlot.outputIndex).SlotContent = Entity.Null;
                }
            }

            for (int outputIndex = 0; outputIndex < buildingAspect.outputSlots.Length; outputIndex++)
            {
                var outputSlot = buildingAspect.outputSlots[outputIndex];
                var currentBuilding = buildingAspect;
                if (!outputSlot.IsConnected || !outputSlot.IsOccupied) continue;

                var nextBuilding = entityManager.GetAspect<BuildingAspect>(outputSlot.EntityToPushTo);

                //push out to next Building
                if (!nextBuilding.inputSlots[outputSlot.InputIndex].IsOccupied)
                {
                    nextBuilding.inputSlots.ElementAt(outputSlot.InputIndex).SlotContent =
                        currentBuilding.outputSlots[outputIndex].SlotContent;
                    currentBuilding.outputSlots.ElementAt(outputIndex).SlotContent = Entity.Null;
                }
            }
        }
    }
}

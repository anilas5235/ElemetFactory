using System.Collections.Generic;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.ItemSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(VariableRateSimulationSystemGroup),OrderFirst = true)]
    public partial struct ChainConveyorSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<ChainPullStartPointTag>();
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

                foreach (ChainStartAspect chainStartAspect in SystemAPI.Query<ChainStartAspect>())
                {
                    if (chainStartAspect.buildingAspect.inputSlots.Length < 1 ||
                        (chainStartAspect.buildingAspect.MyBuildingData.buildingDataID == 1 &&
                         chainStartAspect.buildingAspect.outputSlots[0].IsConnected))
                    {
                        ecb.RemoveComponent<ChainPullStartPointTag>(chainStartAspect.entity);
                        continue;
                    }

                    for (var index = 0; index < chainStartAspect.buildingAspect.inputSlots.Length; index++)
                    {
                        var inputSlot = chainStartAspect.buildingAspect.inputSlots[index];
                        if (!inputSlot.IsConnected) continue;
                        using NativeList<BuildingAspect> buildingChain =
                            new NativeList<BuildingAspect>(Allocator.TempJob);

                        buildingChain.Add(chainStartAspect.buildingAspect);
                        BuildingAspect currentBuilding =
                            SystemAPI.GetAspect<BuildingAspect>(inputSlot.EntityToPullFrom);
                        do
                        {
                            buildingChain.Add(currentBuilding);
                            if (currentBuilding.buildingDataComponent.ValueRO.BuildingData.buildingDataID != 1) break;
                            if (!currentBuilding.inputSlots[0].IsConnected) break;
                            currentBuilding =
                                SystemAPI.GetAspect<BuildingAspect>(currentBuilding.inputSlots[0].EntityToPullFrom);
                        } while (true);

                        chainJobs.Add(new ChainJob()
                        {
                            buildingAspects = new NativeArray<BuildingAspect>(buildingChain.AsArray(),Allocator.TempJob),
                            inputIndex = index,
                            outputIndex = inputSlot.outputIndex,
                        });
                    }
                }

                foreach (ChainJob chainJob in chainJobs)
                {
                    chainJob.Schedule();
                }
                
                rateHandel.timeSinceLastTick = 0;
            }

            SystemAPI.SetComponent(state.SystemHandle,rateHandel);
        }
    }

    [BurstCompile]
    public partial struct ChainJob : IJobEntity
    {
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<BuildingAspect> buildingAspects;
        public int inputIndex, outputIndex;

        private void Execute()
        {
            for (int i = 0; i < buildingAspects.Length - 1; i++)
            {
                var currentBuilding = buildingAspects[i];
                var nextBuilding = buildingAspects[i + 1];

                if (currentBuilding.buildingDataComponent.ValueRO.BuildingData.buildingDataID == 1)
                {
                    if (!currentBuilding.outputSlots[0].IsOccupied && currentBuilding.inputSlots[0].IsOccupied)
                    {
                        currentBuilding.outputSlots.ElementAt(0).SlotContent = currentBuilding.inputSlots[0].SlotContent;
                        currentBuilding.inputSlots.ElementAt(0).SlotContent = Entity.Null;
                    }
                }
                
                if (!currentBuilding.inputSlots[inputIndex].IsOccupied &&
                    nextBuilding.outputSlots[outputIndex].IsOccupied)
                {
                    currentBuilding.inputSlots.ElementAt(inputIndex).SlotContent =
                        nextBuilding.outputSlots[outputIndex].SlotContent;
                    nextBuilding.outputSlots.ElementAt(outputIndex).SlotContent = Entity.Null;
                }

                if (i < buildingAspects.Length - 2)
                {
                    inputIndex = 0;
                    outputIndex = nextBuilding.inputSlots[inputIndex].outputIndex;
                }
            }
        }
    }
}

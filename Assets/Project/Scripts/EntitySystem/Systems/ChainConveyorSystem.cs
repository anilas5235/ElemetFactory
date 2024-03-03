using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.DataObject;
using Project.Scripts.EntitySystem.Components.Item;
using Project.Scripts.ItemSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(VariableRateSimulationSystemGroup),OrderFirst = true)]
    public partial struct ChainConveyorSystem : ISystem
    {
        private EntityQuery otherBuildingsQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BuildingDataComponent>();
            state.EntityManager.AddComponentData(state.SystemHandle, new TimeRateDataComponent()
            {
                timeSinceLastTick = 0,
                Rate = 1f
            });
            
            otherBuildingsQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<BuildingDataComponent>()
                .WithAbsent<ConveyorDataComponent>()
                .Build(ref state);
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var rateHandel = SystemAPI.GetComponent<TimeRateDataComponent>(state.SystemHandle);
            rateHandel.timeSinceLastTick += SystemAPI.Time.DeltaTime;
            if (rateHandel.timeSinceLastTick >= 1f / rateHandel.Rate)
            {
                var ecbs = state.World.GetExistingSystemManaged<EndVariableRateSimulationEntityCommandBufferSystem>();
                var ecb = ecbs.CreateCommandBuffer().AsParallelWriter();

                var dep = new ChainJob()
                {
                    inputsLookup = SystemAPI.GetBufferLookup<InputSlot>(),
                    outputsLookup = SystemAPI.GetBufferLookup<OutputSlot>(),
                    itemStates = SystemAPI.GetComponentLookup<ItemEntityStateDataComponent>(),
                    ECB = ecb,
                }.ScheduleParallel(new JobHandle());

                ecbs.AddJobHandleForProducer(dep);

                rateHandel.timeSinceLastTick = 0;
            }

            SystemAPI.SetComponent(state.SystemHandle, rateHandel);
        }
    }

    [BurstCompile]
    public partial struct ChainJob : IJobEntity
    {
        [NativeDisableContainerSafetyRestriction] public BufferLookup<InputSlot> inputsLookup;
        [NativeDisableContainerSafetyRestriction] public BufferLookup<OutputSlot> outputsLookup;
        [NativeDisableContainerSafetyRestriction] public ComponentLookup<ItemEntityStateDataComponent> itemStates;
        public EntityCommandBuffer.ParallelWriter ECB;

        private int changes;
        private bool firstConnected, lastConnected;
        
        private void Execute([ChunkIndexInQuery]int index,Entity entity,DynamicBuffer<EntityRefBufferElement> chain, ConveyorChainDataComponent chainDataComponent)
        {
            if(chainDataComponent.Sleep) return;
            
            for (var linkIndex = chain.Length - 1; linkIndex >= 0; linkIndex--)
            {
                Entity currentBuilding = chain[linkIndex].Entity;
                DynamicBuffer<InputSlot> currentInput = inputsLookup[currentBuilding];
                DynamicBuffer<OutputSlot> currentOutput = outputsLookup[currentBuilding];

                if (linkIndex == chain.Length - 1)
                {
                    lastConnected = currentOutput[0].IsConnected;
                    if (lastConnected)
                    {
                        Entity targetBuilding = currentOutput[0].EntityToPushTo;
                        DynamicBuffer<InputSlot> targetInput = inputsLookup[targetBuilding];
                        int targetSlotIndex = currentOutput[0].InputIndex;

                        if (currentOutput[0].IsOccupied && !targetInput[targetSlotIndex].IsOccupied)
                        {
                            targetInput.ElementAt(targetSlotIndex).SlotContent = currentOutput[0].SlotContent;
                            currentOutput.ElementAt(0).SlotContent = Entity.Null;
                            UpdateItemState(index, targetInput[targetSlotIndex].SlotContent,
                                targetInput[targetSlotIndex].Position, itemStates, ECB);
                            changes++;
                        }
                    }
                }

                //push input to output
                if (!currentOutput[0].IsOccupied && currentInput[0].IsOccupied)
                {
                    currentOutput.ElementAt(0).SlotContent = currentInput[0].SlotContent;
                    currentInput.ElementAt(0).SlotContent = Entity.Null;
                    UpdateItemState(index, currentOutput[0].SlotContent, currentOutput[0].Position, itemStates, ECB);
                    changes++;
                }

                if (linkIndex > 0)
                {
                    Entity nextBuilding = chain[linkIndex - 1].Entity;
                    DynamicBuffer<OutputSlot> nextOutput = outputsLookup[nextBuilding];
                    //pull form next link
                    if (!currentInput[0].IsOccupied && nextOutput[0].IsOccupied)
                    {
                        currentInput.ElementAt(0).SlotContent = nextOutput[0].SlotContent;
                        nextOutput.ElementAt(0).SlotContent = Entity.Null;
                        UpdateItemState(index, currentInput[0].SlotContent, currentInput[0].Position, itemStates, ECB);
                    }
                }
                else
                {
                    firstConnected = currentInput[0].IsConnected;
                    if (firstConnected)
                    {
                        Entity lastBuilding = currentInput[0].EntityToPullFrom;
                        DynamicBuffer<OutputSlot> lastOutput = outputsLookup[lastBuilding];
                        int lastSlotIndex = currentInput[0].outputIndex;

                        //pull form next link
                        if (!currentInput[0].IsOccupied && lastOutput[lastSlotIndex].IsOccupied)
                        {
                            currentInput.ElementAt(0).SlotContent = lastOutput[lastSlotIndex].SlotContent;
                            lastOutput.ElementAt(lastSlotIndex).SlotContent = Entity.Null;
                            UpdateItemState(index, currentInput[0].SlotContent, currentInput[0].Position, itemStates,
                                ECB);
                            changes++;
                        }
                    }
                }
            }

            if (changes < 1 && !firstConnected && !lastConnected)
            {
                chainDataComponent.Sleep = true;
                ECB.SetComponent(index,entity,chainDataComponent);
            }
        }
        
        public static void UpdateItemState(int index,Entity itemEntity, float3 destination,
            ComponentLookup<ItemEntityStateDataComponent> itemStates, EntityCommandBuffer.ParallelWriter ECB)
        {
            var itemState = itemStates[itemEntity];
            itemState.PreviousPos = itemState.DestinationPos;
            itemState.DestinationPos = destination;
            itemState.Progress = 0;
            itemState.Arrived = false;
            ECB.SetComponent(index, itemEntity, itemState);
        }
    }
}

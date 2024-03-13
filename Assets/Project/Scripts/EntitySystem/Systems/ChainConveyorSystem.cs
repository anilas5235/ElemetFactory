using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.DataObject;
using Project.Scripts.EntitySystem.Components.Item;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(VariableRateSimulationSystemGroup),OrderFirst = true)]
    public partial struct ChainConveyorSystem : ISystem
    {
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

        private int _changes;
        private bool _firstConnected, _lastConnected;
        
        private void Execute([ChunkIndexInQuery]int index,Entity entity,DynamicBuffer<EntityRefBufferElement> chain, ConveyorChainDataComponent chainDataComponent)
        {
            if(chainDataComponent.Sleep) return;
            
            for (var linkIndex = chain.Length - 1; linkIndex >= 0; linkIndex--)
            {
                var currentBuilding = chain[linkIndex].Entity;
                var currentInput = inputsLookup[currentBuilding];
                var currentOutput = outputsLookup[currentBuilding];

                if (linkIndex == chain.Length - 1)
                {
                    _lastConnected = currentOutput[0].IsConnected;
                    if (_lastConnected)
                    {
                        var targetBuilding = currentOutput[0].ConnectedEntity;
                        var targetInput = inputsLookup[targetBuilding];
                        var targetSlotIndex = currentOutput[0].ConnectedIndex;
                        
                        if (currentOutput[0].IsOccupied && !targetInput[targetSlotIndex].IsOccupied)
                        {
                            targetInput.ElementAt(targetSlotIndex).SetSlotContent(currentOutput[0].SlotContent);
                            currentOutput.ElementAt(0).SetSlotContent(default);
                            UpdateItemState(index, targetInput[targetSlotIndex].SlotContent,
                                targetInput[targetSlotIndex].WorldPosition, itemStates, ECB);
                            _changes++;
                        }
                    }
                }

                //push input to output
                if (!currentOutput[0].IsOccupied && currentInput[0].IsOccupied)
                {
                    currentOutput.ElementAt(0).SetSlotContent(currentInput[0].SlotContent);
                    currentInput.ElementAt(0).SetSlotContent(default);
                    UpdateItemState(index, currentOutput[0].SlotContent, currentOutput[0].WorldPosition, itemStates, ECB);
                    _changes++;
                }

                if (linkIndex > 0)
                {
                    var nextBuilding = chain[linkIndex - 1].Entity;
                    var nextOutput = outputsLookup[nextBuilding];
                    //pull form next link
                    if (currentInput[0].IsOccupied || !nextOutput[0].IsOccupied) continue;
                    
                    currentInput.ElementAt(0).SetSlotContent(nextOutput[0].SlotContent);
                    nextOutput.ElementAt(0).SetSlotContent(default);
                    UpdateItemState(index, currentInput[0].SlotContent, currentInput[0].WorldPosition, itemStates, ECB);
                }
                else
                {
                    _firstConnected = currentInput[0].IsConnected;
                    if (!_firstConnected) continue;
                    
                    var lastBuilding = currentInput[0].ConnectedEntity;
                    var lastOutput = outputsLookup[lastBuilding];
                    var lastSlotIndex = currentInput[0].ConnectedIndex;

                    //pull form next link
                    if (currentInput[0].IsOccupied || !lastOutput[lastSlotIndex].IsOccupied) continue;
                    
                    currentInput.ElementAt(0).SetSlotContent(lastOutput[lastSlotIndex].SlotContent);
                    lastOutput.ElementAt(lastSlotIndex).SetSlotContent(default);
                    UpdateItemState(index, currentInput[0].SlotContent, currentInput[0].WorldPosition, itemStates,
                        ECB);
                    _changes++;
                }
            }

            if (_changes >= 1 || _firstConnected || _lastConnected) return;
            
            chainDataComponent.Sleep = true;
            ECB.SetComponent(index,entity,chainDataComponent);
        }

        private static void UpdateItemState(int index,Entity itemEntity, float3 destination,
            ComponentLookup<ItemEntityStateDataComponent> itemStates, EntityCommandBuffer.ParallelWriter ecb)
        {
            var itemState = itemStates[itemEntity];
            itemState.PreviousPos = itemState.DestinationPos;
            itemState.DestinationPos = destination;
            itemState.Progress = 0;
            itemState.Arrived = false;
            ecb.SetComponent(index, itemEntity, itemState);
        }
    }
}

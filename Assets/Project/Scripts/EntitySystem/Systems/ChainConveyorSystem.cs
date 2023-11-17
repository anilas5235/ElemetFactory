using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
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

                new ChainJob()
                {
                    inputsLookup = SystemAPI.GetBufferLookup<InputSlot>(),
                    outputsLookup = SystemAPI.GetBufferLookup<OutputSlot>(),
                    itemStates = SystemAPI.GetComponentLookup<ItemEntityStateDataComponent>(),
                    ECB = ecb,
                }.ScheduleParallel(new JobHandle()).Complete();

                var dep = new PushPullJob()
                {
                    inputsLookup = SystemAPI.GetBufferLookup<InputSlot>(),
                    outputsLookup = SystemAPI.GetBufferLookup<OutputSlot>(),
                    itemStates = SystemAPI.GetComponentLookup<ItemEntityStateDataComponent>(),
                    ECB = ecb,
                }.ScheduleParallel(otherBuildingsQuery, new JobHandle());

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
        
        private void Execute([ChunkIndexInQuery]int index,DynamicBuffer<ConveyorChainDataPoint> chain)
        {
            for (var linkIndex = chain.Length - 1; linkIndex >= 0; linkIndex--)
            {
                Entity currentBuilding = chain[linkIndex].ConveyorEntity;
                DynamicBuffer<InputSlot> currentInput = inputsLookup[currentBuilding];
                DynamicBuffer<OutputSlot> currentOutput = outputsLookup[currentBuilding];

                //push input to output
                if (!currentOutput[0].IsOccupied && currentInput[0].IsOccupied)
                {
                    currentOutput.ElementAt(0).SlotContent = currentInput[0].SlotContent;
                    currentInput.ElementAt(0).SlotContent = Entity.Null;
                    UpdateItemState(index,currentOutput[0].SlotContent,currentOutput[0].Position,itemStates,ECB);
                }
                
                if (linkIndex >0)
                {
                    Entity nextBuilding = chain[linkIndex - 1].ConveyorEntity;
                    DynamicBuffer<OutputSlot> nextOutput = outputsLookup[nextBuilding];

                    //pull form next link
                    if (!currentInput[0].IsOccupied && nextOutput[0].IsOccupied)
                    {
                        currentInput.ElementAt(0).SlotContent = nextOutput[0].SlotContent;
                        nextOutput.ElementAt(0).SlotContent = Entity.Null;
                        UpdateItemState(index,currentInput[0].SlotContent,currentInput[0].Position,itemStates,ECB);
                    }
                }
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


    [BurstCompile]
    public partial struct PushPullJob: IJobEntity
    {
        [NativeDisableContainerSafetyRestriction] public BufferLookup<InputSlot> inputsLookup;
        [NativeDisableContainerSafetyRestriction] public BufferLookup<OutputSlot> outputsLookup;
        [NativeDisableContainerSafetyRestriction] public ComponentLookup<ItemEntityStateDataComponent> itemStates;
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute([ChunkIndexInQuery] int index, Entity entity)
        {
            DynamicBuffer<InputSlot> currentBuildingInputs = inputsLookup[entity];
            DynamicBuffer<OutputSlot> currentBuildingOutputs = outputsLookup[entity];

            for (var inputIndex = 0; inputIndex < currentBuildingInputs.Length; inputIndex++)
            {
                var inputSlot = currentBuildingInputs[inputIndex];

                if (!inputSlot.IsConnected || inputSlot.IsOccupied) continue;

                var nextOutBuffer = outputsLookup[inputSlot.EntityToPullFrom];

                //push out to next Building
                if (!nextOutBuffer[inputSlot.outputIndex].IsOccupied)
                {
                    currentBuildingInputs.ElementAt(inputIndex).SlotContent =
                        nextOutBuffer[inputSlot.outputIndex].SlotContent;
                    nextOutBuffer.ElementAt(inputSlot.outputIndex).SlotContent = Entity.Null;
                    ChainJob.UpdateItemState(index, currentBuildingInputs[inputIndex].SlotContent,
                        currentBuildingInputs[inputIndex].Position, itemStates, ECB);
                }
            }

            for (int outputIndex = 0; outputIndex < currentBuildingOutputs.Length; outputIndex++)
            {
                var outputSlot = currentBuildingOutputs[outputIndex];

                if (!outputSlot.IsConnected || !outputSlot.IsOccupied) continue;

                var nextInBuffer = inputsLookup[outputSlot.EntityToPushTo];

                //push out to next Building
                if (!nextInBuffer[outputSlot.InputIndex].IsOccupied)
                {
                    nextInBuffer.ElementAt(outputSlot.InputIndex).SlotContent =
                        currentBuildingOutputs[outputIndex].SlotContent;
                    currentBuildingOutputs.ElementAt(outputIndex).SlotContent = Entity.Null;
                    ChainJob.UpdateItemState(index, nextInBuffer[outputSlot.InputIndex].SlotContent,
                        nextInBuffer[outputSlot.InputIndex].Position, itemStates, ECB);
                }
            }
        }
    }
}

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
            state.RequireForUpdate<ChainPushStartPoint>();
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



                foreach (ConveyorChainHeadAspect chainStart in SystemAPI.Query<ConveyorChainHeadAspect>())
                {
                    new ChainJob()
                    {
                        ECB = ecb,
                        Chain = chainStart.chainPushStart.ValueRO.Chain,
                        entity = chainStart.entity,
                    }.Schedule();
                }
                rateHandel.timeSinceLastTick = 0;
            }

            SystemAPI.SetComponent(state.SystemHandle,rateHandel);
        }
    }

    [BurstCompile]
    public partial struct ChainJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public Entity entity;
        [NativeDisableContainerSafetyRestriction] public NativeArray<BuildingAspect> Chain;

        private void Execute()
        {
            if (!Chain[0].inputSlots[0].IsConnected)
            {
                ECB.RemoveComponent<ChainPushStartPoint>(entity);
                return;
            }

            for (var index = 0; index < Chain.Length; index++)
            {
                var currentBuilding = Chain[index];

                if (index + 1 < Chain.Length)
                {
                    var nextBuilding = Chain[index + 1];

                    //push out to next Building
                    if (currentBuilding.outputSlots[0].IsOccupied &&
                        !nextBuilding.inputSlots[0].IsOccupied)
                    {
                        nextBuilding.inputSlots.ElementAt(0).SlotContent =
                            currentBuilding.outputSlots[0].SlotContent;
                        currentBuilding.outputSlots.ElementAt(0).SlotContent = Entity.Null;
                    }
                }
                else
                {
                    //TODO:solve for last
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
}

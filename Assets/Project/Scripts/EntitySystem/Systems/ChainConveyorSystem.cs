using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Burst;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(VariableRateSimulationSystemGroup),OrderFirst = true)]
    public partial struct ChainConveyorSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChainPullStartPointTag>();
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
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
                var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();

                new ChainJob()
                {
                    ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged),
                    entityManager = state.EntityManager,
                }.Schedule();

                rateHandel.timeSinceLastTick = 0;
            }

            SystemAPI.SetComponent(state.SystemHandle,rateHandel);
        }
    }

    [BurstCompile]
    public partial struct ChainJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public EntityManager entityManager;

        private void Execute(ChainStartAspect chainStartAspect)
        {
            if (chainStartAspect.buildingAspect.inputSlots.Length < 1 ||
                (chainStartAspect.buildingAspect.MyBuildingData.buildingDataID == 1 &&
                 chainStartAspect.buildingAspect.outputSlots[0].IsConnected))
            {
                ECB.RemoveComponent<ChainPullStartPointTag>(chainStartAspect.entity);
                return;
            }

            for (var index = 0; index < chainStartAspect.buildingAspect.inputSlots.Length; index++)
            {
                var aspectInputSlot = chainStartAspect.buildingAspect.inputSlots[index];
                if (!aspectInputSlot.IsConnected) continue;

                BuildingAspect currentBuilding = chainStartAspect.buildingAspect;
                BuildingAspect nextBuilding = entityManager.GetAspect<BuildingAspect>(aspectInputSlot.EntityToPullFrom);

                if (nextBuilding.outputSlots[aspectInputSlot.outputIndex].IsOccupied && !aspectInputSlot.IsOccupied)
                {
                    currentBuilding.inputSlots.ElementAt(index).SlotContent =
                        nextBuilding.outputSlots[aspectInputSlot.outputIndex].SlotContent;
                    nextBuilding.outputSlots.ElementAt(aspectInputSlot.outputIndex).SlotContent = default;
                }

                currentBuilding = nextBuilding;

                do
                {
                    if (currentBuilding.inputSlots.Length < 1) break;
                    if (currentBuilding.buildingDataComponent.ValueRO.BuildingData.buildingDataID != 1) break;
                    var myInSlot = currentBuilding.inputSlots[0];
                    var myOutSlot = currentBuilding.outputSlots[0];
                    if (!myInSlot.IsConnected) break;
                    nextBuilding =
                        entityManager.GetAspect<BuildingAspect>(currentBuilding.inputSlots[0].EntityToPullFrom);
                    var otherOutSlot = nextBuilding.outputSlots[myInSlot.outputIndex];

                    if (!myOutSlot.IsOccupied && myInSlot.IsOccupied)
                    {
                        currentBuilding.outputSlots.ElementAt(0).SlotContent = myInSlot.SlotContent;
                        currentBuilding.inputSlots.ElementAt(0).SlotContent = default;
                    }

                    if (otherOutSlot.IsOccupied && !myInSlot.IsOccupied)
                    {
                        currentBuilding.inputSlots.ElementAt(0).SlotContent = otherOutSlot.SlotContent;
                        nextBuilding.outputSlots.ElementAt(myInSlot.outputIndex).SlotContent = default;
                    }

                    currentBuilding = nextBuilding;
                } while (true);
            }
        }
    }
}

using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.EntitySystem.Systems;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct BuildingAspect : IAspect
    {
        public readonly Entity entity;
        public readonly RefRO<LocalTransform> transform;
        public readonly DynamicBuffer<InputSlot> inputSlots;
        public readonly DynamicBuffer<OutputSlot> outputSlots;
        public readonly RefRO<BuildingDataComponent> buildingDataComponent;
        public PlacedBuildingData MyBuildingData => buildingDataComponent.ValueRO.BuildingData;
        public LocalTransform Transform => transform.ValueRO;

        #region Connect

        public void TryToConnectBuildings(BuildingAspect otherBuilding)
        {
            if (!ResourcesUtility.GetBuildingData(MyBuildingData.buildingDataID, out BuildingLookUpData myLookUpData))
                return;
            if (!ResourcesUtility.GetBuildingData(otherBuilding.MyBuildingData.buildingDataID,
                    out BuildingLookUpData otherLookUpData)) return;
            int2 chunkDiff = GenerationSystem.GetChunkPosition(Transform.Position)
                             - GenerationSystem.GetChunkPosition(otherBuilding.Transform.Position);
            chunkDiff *= ChunkDataComponent.ChunkSize;

            TryConnectInputs(myLookUpData, otherLookUpData, otherBuilding, chunkDiff);

            TryConnectOutputs(myLookUpData, otherLookUpData, otherBuilding, chunkDiff);
            
            if (MyBuildingData.buildingDataID == 1)HandelConveyors();
        }

        private void TryConnectInputs(BuildingLookUpData myLookUpData, BuildingLookUpData otherLookUpData,
            BuildingAspect otherBuilding, int2 chunkDiff)
        {
            var myInputPortInfoList = myLookUpData.GetInputPortInfo(MyBuildingData.directionID);
            var otherOutputPortInfoList =
                otherLookUpData.GetOutputPortInfo(otherBuilding.MyBuildingData.directionID);
            int2[] otherGridPositionList = ResourcesUtility.GetGridPositionList(otherBuilding.MyBuildingData);

            foreach (PortInstantData myInputPortInstantData in myInputPortInfoList)
            {
                byte currentInputPortID = myInputPortInstantData.portID;
                if (inputSlots[currentInputPortID].IsConnected) continue;

                int2 point = MyBuildingData.origin +
                             PlacedBuildingUtility.FacingDirectionToVector(myInputPortInstantData.direction);

                if (!TryGetBodyID(otherGridPositionList, point, otherBuilding.MyBuildingData, chunkDiff,
                        out int bodyID)) continue;

                PortInstantData[] specificPortList =
                    otherOutputPortInfoList.Where(data => data.portID == bodyID).ToArray();

                foreach (PortInstantData otherOutputPortInstantData in specificPortList)
                {
                    byte currentPortId = otherOutputPortInstantData.portID;
                    if (otherBuilding.outputSlots[currentPortId].IsConnected) continue;

                    if (myInputPortInfoList[currentInputPortID].direction !=
                        PlacedBuildingUtility.GetOppositeDirection(otherOutputPortInstantData.direction)) continue;

                    inputSlots.ElementAt(currentInputPortID).EntityToPullFrom = otherBuilding.entity;
                    inputSlots.ElementAt(currentInputPortID).outputIndex = currentPortId;

                    otherBuilding.outputSlots.ElementAt(currentPortId).EntityToPushTo = entity;
                    otherBuilding.outputSlots.ElementAt(currentPortId).InputIndex = currentPortId;
                    break;
                }
            }
        }

        private void HandelConveyors()
        {
            EntityCommandBuffer ecb = PlacingSystem.beginSimulationEntityCommandBuffer.CreateCommandBuffer(
                World.DefaultGameObjectInjectionWorld.Unmanaged);

            Entity head = default;

            if (inputSlots[0].IsConnected && GenerationSystem._entityManager
                    .GetComponentData<BuildingDataComponent>(inputSlots[0].EntityToPullFrom)
                    .BuildingData.buildingDataID == 1)
            {
                var sourceBuilding =
                    GenerationSystem._entityManager.GetAspect<ConveyorAspect>(inputSlots[0].EntityToPullFrom);
                head = sourceBuilding.conveyorDataComponent.ValueRO.head;

                var buffer = GenerationSystem._entityManager.GetBuffer<ConveyorChainDataPoint>(head);
                buffer.Add(new ConveyorChainDataPoint()
                {
                    ConveyorEntity = entity,
                });

                ecb.SetComponent(entity, new ConveyorDataComponent()
                {
                    head = head,
                });
            }

            if (outputSlots[0].IsConnected && GenerationSystem._entityManager
                    .GetComponentData<BuildingDataComponent>(outputSlots[0].EntityToPushTo)
                    .BuildingData.buildingDataID == 1)
            {
                var destinationBuilding =
                    GenerationSystem._entityManager.GetAspect<ConveyorAspect>(outputSlots[0].EntityToPushTo);

                if (head != default)
                {
                    //Connect two chains
                    var bufferA = GenerationSystem._entityManager.GetBuffer<ConveyorChainDataPoint>(head);
                    Entity head2 = destinationBuilding.conveyorDataComponent.ValueRO.head;
                    var bufferB = GenerationSystem._entityManager.GetBuffer<ConveyorChainDataPoint>(head2);
                    head = CreateChainHead(ecb, bufferA, bufferB,
                        out DynamicBuffer<ConveyorChainDataPoint> newChain);

                    ecb.DestroyEntity(head);
                    ecb.DestroyEntity(head2);

                    foreach (ConveyorChainDataPoint chainLink in newChain)
                    {

                        GenerationSystem._entityManager.SetComponentData(chainLink.ConveyorEntity,
                            new ConveyorDataComponent()
                            {
                                head = head,
                            });
                    }
                }
                else
                {
                    head = destinationBuilding.conveyorDataComponent.ValueRO.head;
                    var buffer = GenerationSystem._entityManager.GetBuffer<ConveyorChainDataPoint>(head);
                    buffer.Insert(0, new ConveyorChainDataPoint()
                    {
                        ConveyorEntity = entity,
                    });
                    ecb.SetComponent(entity, new ConveyorDataComponent()
                    {
                        head = head,
                    });
                }
            }

            if (head == default)
            {
                CreateChainHead(ecb, out var buffer);
                buffer.Add(new ConveyorChainDataPoint()
                {
                    ConveyorEntity = entity,
                });
            }
        }

        private void TryConnectOutputs(BuildingLookUpData myLookUpData, BuildingLookUpData otherLookUpData,
            BuildingAspect otherBuilding, int2 chunkDiff)
        {
            PortInstantData[] myOutputPortInfoList = myLookUpData.GetOutputPortInfo(MyBuildingData.directionID);
            PortInstantData[] otherInputPortInfoList =
                otherLookUpData.GetInputPortInfo(otherBuilding.MyBuildingData.directionID);
            int2[] otherGridPositionList = ResourcesUtility.GetGridPositionList(otherBuilding.MyBuildingData);

            foreach (PortInstantData myOutputPortInstantData in myOutputPortInfoList)
            {
                byte currentOutputPortID = myOutputPortInstantData.portID;
                if (inputSlots[currentOutputPortID].IsConnected) continue;

                int2 point = MyBuildingData.origin +
                             PlacedBuildingUtility.FacingDirectionToVector(myOutputPortInstantData.direction);

                if (!TryGetBodyID(otherGridPositionList, point, otherBuilding.MyBuildingData, chunkDiff,
                        out int bodyID)) continue;

                PortInstantData[] specificPortList =
                    otherInputPortInfoList.Where(data => data.portID == bodyID).ToArray();

                foreach (PortInstantData otherInputPortInstantData in specificPortList)
                {
                    byte currentPortId = otherInputPortInstantData.portID;
                    if (otherBuilding.inputSlots[currentPortId].IsConnected) continue;

                    if (myOutputPortInfoList[currentOutputPortID].direction !=
                        PlacedBuildingUtility.GetOppositeDirection(otherInputPortInstantData.direction)) continue;

                    outputSlots.ElementAt(currentOutputPortID).EntityToPushTo = otherBuilding.entity;
                    outputSlots.ElementAt(currentOutputPortID).InputIndex = currentPortId;

                    otherBuilding.inputSlots.ElementAt(currentPortId).EntityToPullFrom = entity;
                    otherBuilding.inputSlots.ElementAt(currentPortId).outputIndex = currentOutputPortID;
                    break;
                }
            }
        }

        private static bool TryGetBodyID(int2[] gridPositionList, int2 point, PlacedBuildingData otherBuildingData,
            int2 chunkDiff, out int bodyID)
        {
            bodyID = -1;

            for (int j = 0; j < gridPositionList.Length; j++)
            {
                var isSame = point == gridPositionList[j] + otherBuildingData.origin + chunkDiff;
                if (isSame is not { x: true, y: true }) continue;
                bodyID = j;
                break;
            }

            return bodyID >= 0;
        }

        #endregion

        private static Entity CreateChainHead(EntityCommandBuffer ecb,
            DynamicBuffer<ConveyorChainDataPoint>chainA,DynamicBuffer<ConveyorChainDataPoint> chainB,
            out DynamicBuffer<ConveyorChainDataPoint> buffer)
        {
           var entity = ecb.CreateEntity();
           buffer = ecb.AddBuffer<ConveyorChainDataPoint>(entity);
           buffer.AddRange(chainA.AsNativeArray());
           buffer.AddRange(chainB.AsNativeArray());
           ecb.SetName(entity,"ChainHead");
           return entity;
        }
        private static Entity CreateChainHead(EntityCommandBuffer ecb,
            out DynamicBuffer<ConveyorChainDataPoint> buffer)
        {
            var entity = ecb.CreateEntity();
            buffer = ecb.AddBuffer<ConveyorChainDataPoint>(entity);
            ecb.SetName(entity,"ChainHead");
            return entity;
        }
    }
}
using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.DataObject;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.EntitySystem.Systems;
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

        public void TryToConnectBuildings(BuildingAspect otherBuilding, int2 otherBuildingRelativePosition)
        {
            if (!ResourcesUtility.GetBuildingData(MyBuildingData.buildingDataID, out var myLookUpData))
                return;
            if (!ResourcesUtility.GetBuildingData(otherBuilding.MyBuildingData.buildingDataID,
                    out var otherLookUpData)) return;
            var chunkDiff = GenerationSystem.GetChunkPosition(Transform.Position)
                            - GenerationSystem.GetChunkPosition(otherBuilding.Transform.Position);
            chunkDiff *= ChunkDataComponent.ChunkSize;

            TryConnectInputs(myLookUpData, otherLookUpData, otherBuilding, chunkDiff,otherBuildingRelativePosition);

            TryConnectOutputs(myLookUpData, otherLookUpData, otherBuilding, chunkDiff);

            if (otherBuilding.MyBuildingData.buildingDataID != 1) return;
            
            var conv = GenerationSystem.entityManager.GetAspect<ConveyorAspect>(otherBuilding.entity);
            GenerationSystem.entityManager.SetComponentData(conv.conveyorDataComponent.ValueRO.head,
                new ConveyorChainDataComponent()
                {
                    Sleep = false,
                });
        }

        private void TryConnectInputs(BuildingLookUpData myLookUpData, BuildingLookUpData otherLookUpData,
            BuildingAspect otherBuilding, int2 chunkDiff, int2 otherBuildingRelativePosition)
        {
            var myInputPortInfoList = myLookUpData.GetInputPortInfo(MyBuildingData.directionID);
            var otherOutputPortInfoList =
                otherLookUpData.GetOutputPortInfo(otherBuilding.MyBuildingData.directionID);
            var otherGridPositionList = ResourcesUtility.GetGridPositionList(otherBuilding.MyBuildingData);

            foreach (var myInputPortInstantData in myInputPortInfoList)
            {
                if (otherBuilding.MyBuildingData.buildingDataID == 1 && MyBuildingData.buildingDataID ==1)
                {
                    var offset = otherBuildingRelativePosition -
                                 myLookUpData.GetBodyOffset(myInputPortInstantData.bodyPartID, MyBuildingData.directionID);
                    if(!PlacedBuildingUtility.DoYouPointAtMe(otherBuilding.MyBuildingData.directionID,offset))continue;
                }
                var currentInputPortID = myInputPortInstantData.portID;
                if (inputSlots[currentInputPortID].IsConnected) continue;

                var point = MyBuildingData.origin +
                            myLookUpData.GetBodyOffset(myInputPortInstantData.bodyPartID, MyBuildingData.directionID)+
                            PlacedBuildingUtility.FacingDirectionToVector(myInputPortInstantData.direction);

                if (!TryGetBodyID(otherGridPositionList, point, otherBuilding.MyBuildingData, chunkDiff,
                        out var bodyID)) continue;

                var specificPortList =
                    otherOutputPortInfoList.Where(data => data.portID == bodyID).ToArray();

                foreach (var otherOutputPortInstantData in specificPortList)
                {
                    var otherOutputPortID = otherOutputPortInstantData.portID;
                    if (otherBuilding.outputSlots[otherOutputPortID].IsConnected) continue;

                    if (myInputPortInfoList[currentInputPortID].direction !=
                        PlacedBuildingUtility.GetOppositeDirection(otherOutputPortInstantData.direction)) continue;
                    
                    inputSlots.ElementAt(currentInputPortID).SetConnectedEntity(otherBuilding.entity);
                    inputSlots.ElementAt(currentInputPortID).SetConnectedIndex(otherOutputPortID);

                    otherBuilding.outputSlots.ElementAt(otherOutputPortID).SetConnectedEntity(entity);
                    otherBuilding.outputSlots.ElementAt(otherOutputPortID).SetConnectedIndex(currentInputPortID);
                    break;
                }
            }
        }

        private void TryConnectOutputs(BuildingLookUpData myLookUpData, BuildingLookUpData otherLookUpData,
            BuildingAspect otherBuilding, int2 chunkDiff)
        {
            var myOutputPortInfoList = myLookUpData.GetOutputPortInfo(MyBuildingData.directionID);
            var otherInputPortInfoList =
                otherLookUpData.GetInputPortInfo(otherBuilding.MyBuildingData.directionID);
            var otherGridPositionList = ResourcesUtility.GetGridPositionList(otherBuilding.MyBuildingData);

            foreach (var myOutputPortInstantData in myOutputPortInfoList)
            {
                var currentOutputPortID = myOutputPortInstantData.portID;
                if (outputSlots[currentOutputPortID].IsConnected) continue;

                var point = MyBuildingData.origin +
                            PlacedBuildingUtility.FacingDirectionToVector(myOutputPortInstantData.direction);

                if (!TryGetBodyID(otherGridPositionList, point, otherBuilding.MyBuildingData, chunkDiff,
                        out var bodyID)) continue;

                var specificPortList =
                    otherInputPortInfoList.Where(data => data.portID == bodyID).ToArray();

                foreach (var otherInputPortInstantData in specificPortList)
                {
                    var otherInputPortID = otherInputPortInstantData.portID;
                    if (otherBuilding.inputSlots[otherInputPortID].IsConnected) continue;

                    if (myOutputPortInfoList[currentOutputPortID].direction !=
                        PlacedBuildingUtility.GetOppositeDirection(otherInputPortInstantData.direction)) continue;

                    outputSlots.ElementAt(currentOutputPortID).SetConnectedEntity(otherBuilding.entity);
                    outputSlots.ElementAt(currentOutputPortID).SetConnectedIndex(otherInputPortID);

                    otherBuilding.inputSlots.ElementAt(otherInputPortID).SetConnectedEntity(entity);
                    otherBuilding.inputSlots.ElementAt(otherInputPortID).SetConnectedIndex(currentOutputPortID);
                    break;
                }
            }
        }

        private static bool TryGetBodyID(int2[] gridPositionList, int2 point, PlacedBuildingData otherBuildingData,
            int2 chunkDiff, out int bodyID)
        {
            bodyID = -1;

            for (var j = 0; j < gridPositionList.Length; j++)
            {
                var isSame = point == gridPositionList[j] + otherBuildingData.origin + chunkDiff;
                if (isSame is not { x: true, y: true }) continue;
                bodyID = j;
                break;
            }

            return bodyID >= 0;
        }

        #endregion
    }
}
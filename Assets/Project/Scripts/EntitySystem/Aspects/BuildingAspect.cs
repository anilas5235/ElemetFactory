using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.DataObject;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.EntitySystem.Systems;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
            if (!ResourcesUtility.GetBuildingData(MyBuildingData.buildingDataID, out BuildingLookUpData myLookUpData))
                return;
            if (!ResourcesUtility.GetBuildingData(otherBuilding.MyBuildingData.buildingDataID,
                    out BuildingLookUpData otherLookUpData)) return;
            int2 chunkDiff = GenerationSystem.GetChunkPosition(Transform.Position)
                             - GenerationSystem.GetChunkPosition(otherBuilding.Transform.Position);
            chunkDiff *= ChunkDataComponent.ChunkSize;

            TryConnectInputs(myLookUpData, otherLookUpData, otherBuilding, chunkDiff,otherBuildingRelativePosition);

            TryConnectOutputs(myLookUpData, otherLookUpData, otherBuilding, chunkDiff);

            if (otherBuilding.MyBuildingData.buildingDataID == 1)
            {
                var conv = GenerationSystem._entityManager.GetAspect<ConveyorAspect>(otherBuilding.entity);
                GenerationSystem._entityManager.SetComponentData(conv.conveyorDataComponent.ValueRO.head,
                    new ConveyorChainDataComponent()
                    {
                        Sleep = false,
                    });
            }
        }

        private void TryConnectInputs(BuildingLookUpData myLookUpData, BuildingLookUpData otherLookUpData,
            BuildingAspect otherBuilding, int2 chunkDiff, int2 otherBuildingRelativePosition)
        {
            var myInputPortInfoList = myLookUpData.GetInputPortInfo(MyBuildingData.directionID);
            var otherOutputPortInfoList =
                otherLookUpData.GetOutputPortInfo(otherBuilding.MyBuildingData.directionID);
            int2[] otherGridPositionList = ResourcesUtility.GetGridPositionList(otherBuilding.MyBuildingData);

            foreach (PortInstantData myInputPortInstantData in myInputPortInfoList)
            {
                if (otherBuilding.MyBuildingData.buildingDataID == 1 && MyBuildingData.buildingDataID ==1)
                {
                    int2 offset = otherBuildingRelativePosition -
                         myLookUpData.GetBodyOffset(myInputPortInstantData.bodyPartID, MyBuildingData.directionID);
                    if(!PlacedBuildingUtility.DoYouPointAtMe(otherBuilding.MyBuildingData.directionID,offset))continue;
                }
                byte currentInputPortID = myInputPortInstantData.portID;
                if (inputSlots[currentInputPortID].IsConnected) continue;

                int2 point = MyBuildingData.origin +
                             myLookUpData.GetBodyOffset(myInputPortInstantData.bodyPartID, MyBuildingData.directionID)+
                             PlacedBuildingUtility.FacingDirectionToVector(myInputPortInstantData.direction);

                if (!TryGetBodyID(otherGridPositionList, point, otherBuilding.MyBuildingData, chunkDiff,
                        out int bodyID)) continue;

                PortInstantData[] specificPortList =
                    otherOutputPortInfoList.Where(data => data.portID == bodyID).ToArray();

                foreach (PortInstantData otherOutputPortInstantData in specificPortList)
                {
                    byte otherOutputPortID = otherOutputPortInstantData.portID;
                    if (otherBuilding.outputSlots[otherOutputPortID].IsConnected) continue;

                    if (myInputPortInfoList[currentInputPortID].direction !=
                        PlacedBuildingUtility.GetOppositeDirection(otherOutputPortInstantData.direction)) continue;
                    
                    inputSlots.ElementAt(currentInputPortID).EntityToPullFrom = otherBuilding.entity;
                    inputSlots.ElementAt(currentInputPortID).outputIndex = otherOutputPortID;

                    otherBuilding.outputSlots.ElementAt(otherOutputPortID).EntityToPushTo = entity;
                    otherBuilding.outputSlots.ElementAt(otherOutputPortID).InputIndex = currentInputPortID;
                    break;
                }
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
                if (outputSlots[currentOutputPortID].IsConnected) continue;

                int2 point = MyBuildingData.origin +
                             PlacedBuildingUtility.FacingDirectionToVector(myOutputPortInstantData.direction);

                if (!TryGetBodyID(otherGridPositionList, point, otherBuilding.MyBuildingData, chunkDiff,
                        out int bodyID)) continue;

                PortInstantData[] specificPortList =
                    otherInputPortInfoList.Where(data => data.portID == bodyID).ToArray();

                foreach (PortInstantData otherInputPortInstantData in specificPortList)
                {
                    byte otherInputPortID = otherInputPortInstantData.portID;
                    if (otherBuilding.inputSlots[otherInputPortID].IsConnected) continue;

                    if (myOutputPortInfoList[currentOutputPortID].direction !=
                        PlacedBuildingUtility.GetOppositeDirection(otherInputPortInstantData.direction)) continue;

                    outputSlots.ElementAt(currentOutputPortID).EntityToPushTo = otherBuilding.entity;
                    outputSlots.ElementAt(currentOutputPortID).InputIndex = otherInputPortID;

                    otherBuilding.inputSlots.ElementAt(otherInputPortID).EntityToPullFrom = entity;
                    otherBuilding.inputSlots.ElementAt(otherInputPortID).outputIndex = currentOutputPortID;
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
    }
}
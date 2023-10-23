using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Grid;
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

        public void TryToConnect(BuildingAspect otherBuilding)
        {
            if (!ResourcesUtility.GetBuildingData(MyBuildingData.buildingDataID, out BuildingLookUpData myLookUpData))
                return;
            if (!ResourcesUtility.GetBuildingData(otherBuilding.MyBuildingData.buildingDataID,
                    out BuildingLookUpData otherLookUpData)) return;
            int2 chunkDiff = ChunkDataAspect.GetChunkPositionFromWorldPosition(Transform.Position)
                             - ChunkDataAspect.GetChunkPositionFromWorldPosition(otherBuilding.Transform.Position);
            chunkDiff *= ChunkDataComponent.ChunkSize;
            
            
            //Try To connect the Input
            {
                var myInputPortInfoList = myLookUpData.GetInputPortInfo(MyBuildingData.directionID);
                var otherOutputPortInfoList =
                    otherLookUpData.GetOutputPortInfo(otherBuilding.MyBuildingData.directionID);
                int2[]otherGridPositionList =ResourcesUtility.GetGridPositionList(otherBuilding.MyBuildingData);

                for (int i = 0; i < myInputPortInfoList.Length; i++)
                {
                    if (inputSlots[i].IsConnected) continue;
                    int2 point = MyBuildingData.origin + PlacedBuildingUtility.FacingDirectionToVector(myInputPortInfoList[i].direction);
                    int bodyID =-1;

                    for (int j = 0; j < otherGridPositionList.Length; j++)
                    {
                       var isSame = point == otherGridPositionList[j] + otherBuilding.MyBuildingData.origin + chunkDiff;
                       if (isSame is not { x: true, y: true }) continue;
                       bodyID = j;
                       break;
                    }
                    
                    if(bodyID<0) continue;

                    for (int j = 0; j < otherOutputPortInfoList.Length; j++)
                    {
                        if (otherBuilding.outputSlots.ElementAt(j).IsConnected ||
                            otherOutputPortInfoList[j].bodyPartID != bodyID) continue;

                        if (myInputPortInfoList[i].direction !=
                            PlacedBuildingUtility.GetOppositeDirection(otherOutputPortInfoList[i].direction)) continue;

                        inputSlots.ElementAt(i).EntityToPullFrom = otherBuilding.entity;
                        inputSlots.ElementAt(i).outputIndex = j;

                        otherBuilding.outputSlots.ElementAt(j).EntityToPushTo = entity;
                        otherBuilding.outputSlots.ElementAt(j).InputIndex = i;
                        break;
                    }
                }
            }
            
            //Try to connect Output
            {
                PortInstantData[] myOutputPortInfoList = myLookUpData.GetOutputPortInfo(MyBuildingData.directionID);
                PortInstantData[] otherInputPortInfoList =
                    otherLookUpData.GetInputPortInfo(otherBuilding.MyBuildingData.directionID);
                int2[]otherGridPositionList =ResourcesUtility.GetGridPositionList(otherBuilding.MyBuildingData);
                
                for (int i = 0; i < myOutputPortInfoList.Length; i++)
                {
                    if (inputSlots[i].IsConnected) continue;
                    int2 point = MyBuildingData.origin + PlacedBuildingUtility.FacingDirectionToVector(myOutputPortInfoList[i].direction);
                    int bodyID =-1;

                    for (int j = 0; j < otherGridPositionList.Length; j++)
                    {
                        var isSame = point == otherGridPositionList[j] + otherBuilding.MyBuildingData.origin + chunkDiff;
                        if (isSame is not { x: true, y: true }) continue;
                        bodyID = j;
                        break;
                    }
                    
                    if(bodyID<0) continue;

                    for (int j = 0; j < otherInputPortInfoList.Length; j++)
                    {
                        if (otherBuilding.outputSlots.ElementAt(j).IsConnected ||
                            otherInputPortInfoList[j].bodyPartID != bodyID) continue;

                        if (myOutputPortInfoList[i].direction !=
                            PlacedBuildingUtility.GetOppositeDirection(otherInputPortInfoList[i].direction)) continue;

                        inputSlots.ElementAt(i).EntityToPullFrom = otherBuilding.entity;
                        inputSlots.ElementAt(i).outputIndex = j;

                        otherBuilding.outputSlots.ElementAt(j).EntityToPushTo = entity;
                        otherBuilding.outputSlots.ElementAt(j).InputIndex = i;
                        break;
                    }
                }
            }
        }
    }
}
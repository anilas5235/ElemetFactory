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

            int2[] inputsDirections = myLookUpData.GetInputPortDirections(MyBuildingData.directionID);
            for (int i = 0; i < inputsDirections.Length; i++)
            {
                if (inputSlots[i].IsConnected) continue;
                int2 point = MyBuildingData.origin + inputsDirections[i];

                int2[] otherOutputOffsets =
                    otherLookUpData.GetOutputPortDirections(otherBuilding.MyBuildingData.directionID);

                for (int j = 0; j < otherOutputOffsets.Length; j++)
                {
                    int2 transformedPoint =
                        otherOutputOffsets[i] +
                        PlacedBuildingUtility.FacingDirectionToVector(
                            PlacedBuildingUtility.GetOppositeDirection(otherBuilding.MyBuildingData.directionID))
                        + otherBuilding.MyBuildingData.origin + chunkDiff;
                    var isSame = point == transformedPoint;
                    if (isSame is not { x: true, y: true }) continue;
                    inputSlots.ElementAt(i).EntityToPullFrom = otherBuilding.entity;
                    inputSlots.ElementAt(i).outputIndex = j;

                    otherBuilding.outputSlots.ElementAt(j).EntityToPushTo = entity;
                    otherBuilding.outputSlots.ElementAt(j).InputIndex = i;
                    break;
                }
            }

            var outputDirections = myLookUpData.GetOutputPortDirections(MyBuildingData.directionID);
            for (int i = 0; i < outputDirections.Length; i++)
            {
                if (outputSlots[i].IsOccupied) continue;
                int2 point = MyBuildingData.origin + outputDirections[i];

                int2[] otherInputOffsets =
                    otherLookUpData.GetInputPortDirections(otherBuilding.MyBuildingData.directionID);

                for (int j = 0; j < otherInputOffsets.Length; j++)
                {
                    int2 transformedPoint = otherInputOffsets[i] +
                                            PlacedBuildingUtility.FacingDirectionToVector(otherBuilding.MyBuildingData
                                                .directionID) + otherBuilding.MyBuildingData.origin + chunkDiff;
                    var isSame = point == transformedPoint;
                    if (isSame is not { x: true, y: true }) continue;

                    outputSlots.ElementAt(i).EntityToPushTo = otherBuilding.entity;
                    outputSlots.ElementAt(i).InputIndex = j;

                    otherBuilding.inputSlots.ElementAt(j).EntityToPullFrom = entity;
                    otherBuilding.inputSlots.ElementAt(j).outputIndex = i;
                    break;
                }
            }
        }
    }
}
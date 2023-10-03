using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Grid;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.SlotSystem
{
    public enum SlotBehaviour
    {
        InAndOutput,
        Input,
        Output
    }
    
    [CreateAssetMenu(menuName = "BuildingSystem/SlotValidationHandler")]
    public class SlotValidationHandler : ScriptableObject
    {
        [SerializeField] private FacingDirection ownFacingDirection;
        [SerializeField] private int2[] validInputPositions;
        [SerializeField] private int2[] validOutputPositions;

        public int2[] ValidInputPositions => validInputPositions;
        public int2[] ValidOutputPositions => validOutputPositions;

        public bool ValidateInputSlotRequest(PlacedBuildingEntity me, PlacedBuildingEntity requester, out int index)
        {
            return ValidateSlotRequest(me, requester, out index, true);
        }

        public bool ValidateOutputSlotRequest(PlacedBuildingEntity me, PlacedBuildingEntity requester, out int index)
        {
            return ValidateSlotRequest(me, requester, out index, false);
        }

        private bool ValidateSlotRequest(PlacedBuildingEntity me, PlacedBuildingEntity requester, out int index, bool input)
        {
            index = 0;

            if (requester.MyPlacedBuildingData.directionID != (int)ownFacingDirection &&
                (PossibleBuildings)requester.MyPlacedBuildingData.buildingDataID != PossibleBuildings.Conveyor)
                return false;

            int2 chunkOffset = me.MyCellObject.ChunkPosition - requester.MyCellObject.ChunkPosition;
            int2 newPos = requester.MyCellObject.Position - chunkOffset * ChunkDataComponent.ChunkSize;

            int2 search = newPos - me.MyCellObject.Position;

            int2[] validSlots = input ? validInputPositions : validOutputPositions;

            for (int i = 0; i < validSlots.Length; i++)
            {
                if (validSlots[i].x != search.x ||validSlots[i].y != search.y) continue;
                index = i;
                return true;
            }

            return false;
        }
    }
}

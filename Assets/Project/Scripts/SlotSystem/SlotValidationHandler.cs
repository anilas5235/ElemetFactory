using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Grid;
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
        [SerializeField] private Vector2Int[] validInputPositions;
        [SerializeField] private Vector2Int[] validOutputPositions;

        public Vector2Int[] ValidInputPositions => validInputPositions;
        public Vector2Int[] ValidOutputPositions => validOutputPositions;

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

            Vector2Int chunkOffset = me.MyGridObject.Chunk.ChunkPosition - requester.MyGridObject.Chunk.ChunkPosition;
            Vector2Int newPos = requester.MyGridObject.Position - chunkOffset * ChunkDataComponent.ChunkSize;

            Vector2Int search = newPos - me.MyGridObject.Position;

            Vector2Int[] validSlots = input ? validInputPositions : validOutputPositions;

            for (int i = 0; i < validSlots.Length; i++)
            {
                if (validSlots[i] != search) continue;
                index = i;
                return true;
            }

            return false;
        }
    }
}

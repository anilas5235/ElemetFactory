using System.Linq;
using Project.Scripts.Buildings;
using Project.Scripts.Grid;
using UnityEngine;

namespace Project.Scripts.SlotSystem
{
    [CreateAssetMenu(menuName = "BuildingSystem/SlotValidationHandler")]
    public class SlotValidationHandler : ScriptableObject
    {
        [SerializeField] private FacingDirection ownFacingDirection;
        [SerializeField] private Vector2Int[] validInputPositions;
        [SerializeField] private Vector2Int[] validOutputPositions;

        public Vector2Int[] ValidInputPositions => validInputPositions;
        public Vector2Int[] ValidOutputPositions => validOutputPositions;

        public bool ValidateInputSlotRequest(PlacedBuilding me, PlacedBuilding requester, out int index)
        {
            return ValidateSlotRequest(me, requester, out index, true);
        }

        public bool ValidateOutputSlotRequest(PlacedBuilding me, PlacedBuilding requester, out int index)
        {
            return ValidateSlotRequest(me, requester, out index, false);
        }

        private bool ValidateSlotRequest(PlacedBuilding me, PlacedBuilding requester, out int index, bool input)
        {
            index = 0;
            if (requester.MyPlacedBuildingData.directionID != (int)ownFacingDirection) return false;

            Vector2Int chunkOffset = me.MyChunk.ChunkPosition - requester.MyChunk.ChunkPosition;
            Vector2Int newPos = requester.MyGridObject.Position - chunkOffset * GridBuildingSystem.ChunkSize;

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

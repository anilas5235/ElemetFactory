using System;
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

        public bool ValidateInputSlotRequest(Vector2Int ownOrigin, Vector2Int originOfRequest,
            FacingDirection requestDirection)
        {
            if (ownOrigin.y >= GridBuildingSystem.ChunkSize.y-1) ownOrigin.y -= GridBuildingSystem.ChunkSize.y;
            else if (ownOrigin.y <= 0) ownOrigin.y += GridBuildingSystem.ChunkSize.y;
            if (ownOrigin.x >= GridBuildingSystem.ChunkSize.x-1) ownOrigin.x -= GridBuildingSystem.ChunkSize.x;
            else if (ownOrigin.x <= 0) ownOrigin.x += GridBuildingSystem.ChunkSize.x;

            Vector2Int search = originOfRequest - ownOrigin;
            return (validInputPositions.Contains(search) && requestDirection == ownFacingDirection);
        }

        public bool ValidateOutputSlotRequest(Vector2Int ownOrigin,Vector2Int originOfRequest, FacingDirection requestDirection)
        {
            Vector2Int search = originOfRequest - ownOrigin;
            return validOutputPositions.Contains(search)&& requestDirection == ownFacingDirection;
        }
    }
}

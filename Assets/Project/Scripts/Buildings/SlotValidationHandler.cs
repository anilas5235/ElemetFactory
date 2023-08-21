using System.Linq;
using Project.Scripts.Grid;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    [CreateAssetMenu(menuName = "BuildingSystem/SlotValidationHandler")]
    public class SlotValidationHandler : ScriptableObject
    {
        [SerializeField] private BuildingScriptableData.FacingDirection ownFacingDirection;
        [SerializeField] private Vector2Int[] validInputPositions;
        [SerializeField] private Vector2Int[] validOutputPositions;

        public Vector2Int[] ValidInputPositions => validInputPositions;
        public Vector2Int[] ValidOutputPositions => validOutputPositions;

        public bool ValidateInputSlotRequest(Vector2Int ownOrigin,Vector2Int originOfRequest, BuildingScriptableData.FacingDirection requestDirection)
        {
            Vector2Int search = originOfRequest - ownOrigin;
            return validInputPositions.Contains(search)&& requestDirection == ownFacingDirection;
        }
        
        public bool ValidateOutputSlotRequest(Vector2Int ownOrigin,Vector2Int originOfRequest, BuildingScriptableData.FacingDirection requestDirection)
        {
            Vector2Int search = originOfRequest - ownOrigin;
            return validOutputPositions.Contains(search)&& requestDirection == ownFacingDirection;
        }
    }
}

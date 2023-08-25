using System.Linq;
using Project.Scripts.Buildings;
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

        public bool ValidateInputSlotRequest(Vector2Int ownOrigin,Vector2Int originOfRequest,FacingDirection requestDirection)
        {
            Vector2Int search = originOfRequest - ownOrigin;
            return validInputPositions.Contains(search)&& requestDirection == ownFacingDirection;
        }
        
        public bool ValidateOutputSlotRequest(Vector2Int ownOrigin,Vector2Int originOfRequest, FacingDirection requestDirection)
        {
            Vector2Int search = originOfRequest - ownOrigin;
            return validOutputPositions.Contains(search)&& requestDirection == ownFacingDirection;
        }
    }
}

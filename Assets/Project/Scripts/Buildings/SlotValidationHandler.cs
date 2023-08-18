using System.Linq;
using Project.Scripts.Grid;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    [CreateAssetMenu(menuName = "BuildingSystem/SlotValidationHandler")]
    public class SlotValidationHandler : ScriptableObject
    {
        [SerializeField] private Vector2Int[] validInputPositions;
        [SerializeField] private Vector2Int[] validOutputPositions;

        public Vector2Int[] ValidInputPositions => validInputPositions;
        public Vector2Int[] ValidOutputPositions => validOutputPositions;

        public bool ValidateInputSlotRequest(Vector2Int ownOrigin,Vector2Int originPseudoOfRequest)
        {
            Vector2Int search = originPseudoOfRequest - ownOrigin;
            return validInputPositions.Contains(search);
        }
        
        public bool ValidateOutputSlotRequest(Vector2Int ownOrigin,Vector2Int originOfRequest)
        {
            Vector2Int search = originOfRequest - ownOrigin;
            return validOutputPositions.Contains(search);
        }
    }
}

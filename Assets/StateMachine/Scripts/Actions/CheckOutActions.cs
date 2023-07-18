using System;
using Project.Scripts.Utilities;
using UnityEngine;

namespace StateMachine.Scripts.Actions
{
    [CreateAssetMenu(menuName = "AI/Actions/CheckOutActions")]
    public class CheckOutActions : Action
    {
        [SerializeField] private ActionType actionType;
        private enum ActionType
        {
            MouseToTargetPosition,
            TargetOldPosition,
            ContinuePatrol,
        }
        public override void Act(StateController controller)
        {
            switch (actionType)
            {
                case ActionType.MouseToTargetPosition:
                    controller.oldPosition = controller.transform.position;
                    controller.targetPosition = GeneralUtilities.GetMousePosition();
                    break;
                case ActionType.TargetOldPosition:
                    controller.targetPosition = controller.oldPosition;
                    break;
                case ActionType.ContinuePatrol:
                    controller.targetPosition = controller.patrolPoints[controller.patrolIndex].position;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

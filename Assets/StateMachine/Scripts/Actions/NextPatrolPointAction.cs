using UnityEngine;

namespace StateMachine.Scripts.Actions
{
    [CreateAssetMenu(menuName = "AI/Actions/NextPatrolPointAction")]
    public class NextPatrolPointAction : Action
    {
        public override void Act(StateController controller)
        {
            controller.patrolIndex++;
            if (controller.patrolIndex > controller.patrolPoints.Length - 1) controller.patrolIndex = 0;
            controller.targetPosition = controller.patrolPoints[controller.patrolIndex].position;
        }
    }
}

using UnityEngine;

namespace StateMachine.Scripts.Actions
{
    [CreateAssetMenu(menuName = "AI/Actions/MoveToTargetAction")]
    public class MoveToTargetAction : Action
    {
        public override void Act(StateController controller)
        {
            controller.transform.position +=(controller.targetPosition - controller.transform.position).normalized * (Time.deltaTime * controller.speed);
        }
    }
}

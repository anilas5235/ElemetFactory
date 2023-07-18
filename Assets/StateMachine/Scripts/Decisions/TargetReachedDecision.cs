using UnityEngine;

namespace StateMachine.Scripts.Decisions
{
    [CreateAssetMenu(menuName = "AI/Decisions/TargetReachedDecision")]
    public class TargetReachedDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            return Vector3.Distance(controller.targetPosition, controller.transform.position) <= 0.01f;
        }
    }
}

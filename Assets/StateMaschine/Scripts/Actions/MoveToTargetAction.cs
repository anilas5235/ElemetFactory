using UnityEngine;

namespace StateMaschine.Scripts.Actions
{
    [CreateAssetMenu(menuName = "AI/Actions/MoveToTargetAction")]
    public class MoveToTargetAction : Action
    {
        public override void Act(StateController controller)
        {
            controller.transform.Translate((controller.targetPosition - controller.transform.position).normalized * (Time.deltaTime * controller.speed));
        }
    }
}

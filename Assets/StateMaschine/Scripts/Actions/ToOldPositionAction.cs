using UnityEngine;

namespace StateMaschine.Scripts.Actions
{
    [CreateAssetMenu(menuName = "AI/Actions/ToOldPositionAction")]
    public class ToOldPositionAction : Action
    {
        public override void Act(StateController controller)
        {
            controller.targetPosition = controller.oldPosition;
        }
    }
}

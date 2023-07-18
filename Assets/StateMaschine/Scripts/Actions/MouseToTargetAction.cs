using Project.Scripts.Utilities;
using UnityEngine;

namespace StateMaschine.Scripts.Actions
{
    [CreateAssetMenu(menuName = "AI/Actions/MouseToTargetAction")]
    public class MouseToTargetAction : Action
    {
        public override void Act(StateController controller)
        {
            controller.oldPosition = controller.targetPosition;
            controller.targetPosition = GeneralUtilities.GetMousePosition();
        }
    }
}

using UnityEngine;

namespace StateMachine.Scripts.Actions
{
    [CreateAssetMenu(menuName = "AI/Actions/RotateAction")]
    public class RotateAction : Action
    {
        public override void Act(StateController controller)
        {
            controller.transform.Rotate(Vector3.forward, controller.rotationSpeed);
        }
    }
}

using UnityEngine;

namespace StateMachine.Scripts.Actions
{
    [CreateAssetMenu(menuName = "AI/Actions/RotateToTarget")]
    public class RotateToTarget : Action
    {
        public override void Act(StateController controller)
        {
            Vector3 direction = (controller.targetPosition - controller.transform.position).normalized;
            Vector3 dif =direction - controller.transform.right;
            if((dif).magnitude<0.05f)return;
            
            controller.transform.right += (dif).normalized * (Time.deltaTime * dif.magnitude * controller.rotationSpeed);
        }
    }
}

using UnityEngine;

namespace StateMachine.Scripts.Actions
{
    [CreateAssetMenu(menuName = "AI/Actions/RotateZ")]
    public class RotateZ : Action
    {
        public bool counterClock;
        public override void Act(StateController controller)
        {
            int multi = counterClock ? -1 : 1;
            
            controller.transform.Rotate(Vector3.forward,  multi* Time.deltaTime * controller.rotationSpeed*5f);

        }
    }
}
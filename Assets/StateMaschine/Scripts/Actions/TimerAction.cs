using UnityEngine;

namespace StateMaschine.Scripts.Actions
{
    [CreateAssetMenu(menuName = "AI/Actions/TimerAction")]
    public class TimerAction : Action
    {
        public override void Act(StateController controller)
        {
            controller.currentTimer += Time.deltaTime;
        }
    }
}
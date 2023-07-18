using UnityEngine;

namespace StateMaschine.Scripts.Decisions
{
    [CreateAssetMenu(menuName = "AI/Decisions/TimerDecision")]
    public class TimerDecision : Decision
    {
        public float timerThreshold;
    
        public override bool Decide(StateController controller)
        {
            return controller.currentTimer >= timerThreshold;
        }
    }
}

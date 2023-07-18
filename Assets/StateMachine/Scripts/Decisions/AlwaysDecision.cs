using UnityEngine;

namespace StateMachine.Scripts.Decisions
{
    [CreateAssetMenu(menuName = "AI/Decisions/AlwaysDecision")]
    public class AlwaysDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            return true;
        }
    }
}

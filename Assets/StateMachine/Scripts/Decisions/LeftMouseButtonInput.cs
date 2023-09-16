using Project.Scripts.EntitySystem.Components.Transmission;
using UnityEngine;

namespace StateMachine.Scripts.Decisions
{
    [CreateAssetMenu(menuName = "AI/Decisions/LeftMouseButtonInput ")]
    public class LeftMouseButtonInput : Decision
    {
        public override bool Decide(StateController controller)
        {
            return Input.GetMouseButtonDown(0);
        }
    }
}

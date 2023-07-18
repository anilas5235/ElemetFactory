using UnityEngine;

namespace StateMaschine.Scripts
{
    [CreateAssetMenu(menuName = "AI/State")]
    public class State : ScriptableObject
    {
        public Action[] actions, enterStateActions, exitStateActions;
        public Transition[] transitions;
    
        public void UpdateState(StateController controller)
        {
            DoActions(controller);
            CheckTransitions(controller);
        }

        public void EnterState(StateController controller)
        {
            foreach (Action action in enterStateActions)
            {
                action.Act(controller);
            }
        }
        
        public void ExitState(StateController controller)
        {
            foreach (Action action in exitStateActions)
            {
                action.Act(controller);
            }
        }

        void DoActions(StateController controller)
        {
            foreach (Action action in actions)
            {
                action.Act(controller);
            }
        }

        void CheckTransitions(StateController controller)
        {
            foreach (Transition transition in transitions)
            {
                bool decision = transition.decision.Decide(controller);
                Debug.Log($"{decision}");
                State targetState = decision ? transition.trueState : transition.falseState;
                controller.TransitionToState(targetState);
            }
        }
    }
}

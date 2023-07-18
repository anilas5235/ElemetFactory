using UnityEngine;

namespace StateMachine.Scripts
{
    [CreateAssetMenu(menuName = "AI/State")]
    public class State : ScriptableObject
    {
        public Action[] enterStateActions, actions, exitStateActions;
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
                if(action) action.Act(controller);
            }
        }
        
        public void ExitState(StateController controller)
        {
            foreach (Action action in exitStateActions)
            {
                if(action) action.Act(controller);
            }
        }

        void DoActions(StateController controller)
        {
            foreach (Action action in actions)
            {
                if(action) action.Act(controller);
            }
        }

        void CheckTransitions(StateController controller)
        {
            foreach (Transition transition in transitions)
            {
                bool decision = transition.decision.Decide(controller);
                State targetState = decision ? transition.trueState : transition.falseState;
                controller.TransitionToState(targetState);
            }
        }
    }
}

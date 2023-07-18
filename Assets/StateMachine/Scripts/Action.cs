using UnityEngine;

namespace StateMachine.Scripts
{
    public abstract class Action : ScriptableObject
    {
        public abstract void Act(StateController controller);
    }
}

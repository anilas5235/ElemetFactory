using UnityEngine;

namespace StateMachine.Scripts
{
    public abstract class Decision : ScriptableObject
    {
        public abstract bool Decide(StateController controller);
    }
}

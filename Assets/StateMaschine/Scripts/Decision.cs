using UnityEngine;

namespace StateMaschine.Scripts
{
    public abstract class Decision : ScriptableObject
    {
        public abstract bool Decide(StateController controller);
    }
}

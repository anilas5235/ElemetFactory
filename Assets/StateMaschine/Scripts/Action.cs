using UnityEngine;

namespace StateMaschine.Scripts
{
    public abstract class Action : ScriptableObject
    {
        public abstract void Act(StateController controller);
    }
}

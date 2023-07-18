using UnityEngine;

namespace StateMaschine.Scripts
{
    public  class StateController : MonoBehaviour
    {
        public float rotationSpeed = 1f;
        public float speed = 2f;

        public Vector3 targetPosition, oldPosition;
        public int patrolIndex;
        public Transform[] patrolPoints;

        public State initialState, userClick;
        private State currentState;

        [HideInInspector]
        public float currentTimer;
    
        private void Start()
        {
            currentState = initialState;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) TransitionToState(userClick);

            currentState.UpdateState(this);
        }

        public void TransitionToState(State targetState)
        {
            if (targetState == null || targetState == currentState) return;

            currentTimer = 0;
            currentState.ExitState(this);
            currentState = targetState;
            currentState.EnterState(this);
        }
    }
}

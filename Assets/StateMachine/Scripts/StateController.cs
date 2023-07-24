using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StateMachine.Scripts
{
    public  class StateController : MonoBehaviour
    {
        private static State Nothing;
        
        public float rotationSpeed = 1f;
        public float speed = 2f;

        public Vector3 targetPosition, oldPosition;
        public int patrolIndex;
        public Transform[] patrolPoints;

        public State initialState, anyState;
        private State currentState;

        [HideInInspector]
        public float currentTimer;

        private void Awake()
        {
            Nothing ??= Resources.Load<State>("AI/States/Nothing");
            patrolIndex = Random.Range(0, patrolPoints.Length);
        }

        private void Start()
        {
            currentState = initialState;
        }

        private void Update()
        {
            currentState.UpdateState(this);
            if(anyState) anyState.UpdateState(this);
        }

        public void TransitionToState(State targetState)
        {
            if (targetState == null) return;
            currentTimer = 0;
            currentState.ExitState(this);
            currentState = targetState;
            currentState.EnterState(this);
        }
    }
}

using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Grid;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Experimental
{
    public class EntityTest : MonoBehaviour
    {
        public PossibleBuildings building;
        public FacingDirection direction;

        private Entity _entity;
        private static EntityManager _entityManager;

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_entity != default) _entityManager.DestroyEntity(_entity);
            }
        }
    }
}

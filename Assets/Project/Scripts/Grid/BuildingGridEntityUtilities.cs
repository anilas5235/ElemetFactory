using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Project.Scripts.Grid
{
    public static class BuildingGridEntityUtilities
    {
        private static EntityManager _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        public static Entity prefapHandler;

        public static Entity CreateBuildingEntity(Vector3 worldPosition, PlacedBuildingData data)
        {
            if (prefapHandler == default) return default;
            var prefaps = _entityManager.GetComponentData<PrefapsDataComponent>(prefapHandler);
            Entity prefap = (PossibleBuildings) data.buildingDataID switch
            {
                PossibleBuildings.Extractor => prefaps.Excavator,
                PossibleBuildings.Conveyor => prefaps.Conveyor,
                PossibleBuildings.Combiner => prefaps.Combiner,
                PossibleBuildings.TrashCan => prefaps.TrashCan,
                PossibleBuildings.Separator => prefaps.Separator,
                _ => throw new ArgumentOutOfRangeException()
            };

            Entity entity = _entityManager.Instantiate(prefap);
            
            var rotation = quaternion.RotateZ(math.radians(PlacedBuildingUtility.GetRotation(data.directionID)));
            _entityManager.SetComponentData(entity, new LocalTransform()
            {
                Position = worldPosition,
                Scale = GenerationSystem.WorldScale,
                Rotation = rotation,
            });

            return entity;
        }
    }
}

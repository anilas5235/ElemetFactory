using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(GenerationSystem))]
    public partial struct PlacingSystem : ISystem
    {
        public static PlacingSystem Instance;
        private static EntityManager _entityManager => GenerationSystem._entityManager;

        public void OnCreate(ref SystemState state)
        {
            Instance = this;
            state.RequireForUpdate<PrefabsDataComponent>();
            state.RequireForUpdate<WorldDataComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            ResourcesUtility.SetUpBuildingData(
                GenerationSystem._entityManager.GetComponentData<PrefabsDataComponent>(GenerationSystem.prefabsEntity));
            state.Enabled = false;
        }

        public bool TryToDeleteBuilding(float3 mousePos)
        {
            if (GenerationSystem.TryGetChunk(GenerationSystem.GetChunkPosition(mousePos),
                    out ChunkDataAspect chunkDataAspect))
            {
               return TryToDeleteBuilding(chunkDataAspect, mousePos);
            }

            return false;
        }

        private bool TryToDeleteBuilding(ChunkDataAspect chunkDataAspect, float3 mousePos)
        {
            return chunkDataAspect.GetCell(ChunkDataAspect.GetCellPositionFormWorldPosition(mousePos, out int2 chunkPosition),
                chunkDataAspect.ChunksPosition).DeleteBuilding();
        }

        public bool TryToPlaceBuilding(float3 mousePos, int buildingID, FacingDirection facingDirection)
        {
            int2 cellPos = ChunkDataAspect.GetCellPositionFormWorldPosition(mousePos, out int2 chunkPosition);
            
            if (GenerationSystem.TryGetChunk(chunkPosition, out ChunkDataAspect chunkDataAspect))
            {
               return chunkDataAspect.TryToPlaceBuilding(buildingID,cellPos,facingDirection);
            }
            return false;
        }

        public static Entity PlaceBuilding( CellObject targetCell, int buildingID, FacingDirection facingDirection)
        {
            if (!ResourcesUtility.GetBuildingData(buildingID, out BuildingData data)) return default;
            
           Entity entity = _entityManager.Instantiate(data.Prefab);
            
           quaternion rotation = quaternion.RotateZ(math.radians(PlacedBuildingUtility.GetRotation(facingDirection)));
         
           _entityManager.SetComponentData(entity, new LocalTransform()
           {
               Position = targetCell.WorldPosition,
               Scale = GenerationSystem.WorldScale,
               Rotation = rotation,
           });
           _entityManager.SetName(entity,data.Name);
           
           //TODO: port setup get input get output ...
           
           return entity;
        }
    }
}

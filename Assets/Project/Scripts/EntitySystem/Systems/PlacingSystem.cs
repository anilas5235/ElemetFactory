using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial struct PlacingSystem : ISystem
    {
        public static PlacingSystem Instance;
        private static EntityManager _entityManager => GenerationSystem._entityManager;

        public void OnCreate(ref SystemState state)
        {
            Instance = this;
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
            return chunkDataAspect.GetCell(ChunkDataAspect.GetCellPositionFormWorldPosition(mousePos),
                chunkDataAspect.ChunksPosition).DeleteBuilding();
        }

        public bool TryToPlaceBuilding(float3 mousePos, int buildingID, FacingDirection facingDirection)
        {
            if (GenerationSystem.TryGetChunk(GenerationSystem.GetChunkPosition(mousePos),
                    out ChunkDataAspect chunkDataAspect))
            {
               return TryToPlaceBuilding(chunkDataAspect, buildingID, mousePos, facingDirection);
            }
            return false;
        }

        public bool TryToPlaceBuilding(ChunkDataAspect chunkDataAspect, int buildingID, float3 mousePos,
            FacingDirection facingDirection)
        {
            CellObject cell = chunkDataAspect.GetCell(ChunkDataAspect.GetCellPositionFormWorldPosition(mousePos),
                chunkDataAspect.ChunksPosition);
            if (cell.IsOccupied) return false;

            PlaceBuilding(chunkDataAspect,cell,buildingID,facingDirection);
            return true;
        }

        public Entity PlaceBuilding(ChunkDataAspect chunkDataAspect, CellObject targetCell, int buildingID,
            FacingDirection facingDirection)
        {
           BuildingData data = ResourcesUtility.GetBuildingData(buildingID);
           Entity entity = _entityManager.Instantiate(data.Prefab);
            
           quaternion rotation = quaternion.RotateZ(math.radians(PlacedBuildingUtility.GetRotation(facingDirection)));
         
           _entityManager.SetComponentData(entity, new LocalTransform()
           {
               Position = targetCell.WorldPosition,
               Scale = GenerationSystem.WorldScale,
               Rotation = rotation,
           });
          
           PlacedBuildingData placedBuildingData = new PlacedBuildingData() //TODO: Put this on a component on the entity
           {
               directionID = buildingID,
               buildingDataID = (int)facingDirection,
               origin = targetCell.Position,
           };

           foreach (int2 pos in ResourcesUtility.GetGridPositionList(placedBuildingData))
           {
               chunkDataAspect.GetCell(pos, chunkDataAspect.ChunksPosition).PlaceBuilding(entity);
           }
           
           //TODO: port setup get input get output ...
           
           return entity;
        }
    }
}

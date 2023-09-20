using System;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Buildings;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Project.Scripts.Grid.DataContainers
{
    public class GridChunk : MonoBehaviour
    {
        public GridField<GridObject> ChunkBuildingGrid { get; private set; }
        public List<PlacedBuildingEntity> Buildings { get; private set; } = new List<PlacedBuildingEntity>();

        public GridBuildingSystem myGridBuildingSystem;
        public Tilemap ChunkTilemap { get; private set; }
        public ChunkResourcePatch[] ChunkResourcePatches { get; private set; }

        private TilemapRenderer chunkTilemapRenderer;
        public Vector3 LocalPosition { get; private set; }
        public Vector2Int ChunkPosition { get; private set; }
        public bool Loaded { get; private set; } = true;

        public void Initialization(GridBuildingSystem gridBuildingSystem, Vector2Int chunkPosition, Vector3 localPosition, ChunkResourcePatch[] resourcePatches = null)
        {
            myGridBuildingSystem = gridBuildingSystem;
            ChunkPosition = chunkPosition;
            LocalPosition = localPosition;
            ChunkResourcePatches = resourcePatches;
            transform.localPosition = LocalPosition;
            ChunkTilemap = GetComponentInChildren<Tilemap>();
            chunkTilemapRenderer = GetComponentInChildren<TilemapRenderer>();
            ChunkBuildingGrid = new GridField<GridObject>(GridBuildingSystem.ChunkSize,GridBuildingSystem.CellSize, transform,
                (field, pos) => new GridObject(this, pos));

            ChunkResourcePatches ??= BuildingGridResources.GenerateResources(this,gridBuildingSystem.Chunks);
            
            foreach (ChunkResourcePatch chunkResourcePatch in ChunkResourcePatches)
            {
                var resourceType = (ResourceType)chunkResourcePatch.resourceID;
                foreach (Vector2Int position in chunkResourcePatch.positions)
                {
                    ChunkBuildingGrid.GetCellData(position).SetResource(resourceType);
                }
            }
        }
        public void LoadBuildingsFromSave(PlacedBuildingData[] buildings)
        {
            if (buildings == null) return;
            foreach (PlacedBuildingData building in buildings)
            {
                StartCoroutine(JobPlaceBuilding(this, building.origin,
                    (PossibleBuildings) building.buildingDataID,(FacingDirection) building.directionID));
            }
        }

        private IEnumerator JobPlaceBuilding(GridChunk chunk, Vector2Int cellPos, PossibleBuildings buildingData,
            FacingDirection facingDirection)
        {
            PlaceBuilding(chunk,cellPos,buildingData,facingDirection);
            yield return null;
        }

        public static void TryToPlaceBuilding( GridChunk chunk,PossibleBuildings  buildingData, Vector3 mousePosition,
            FacingDirection facingDirection)
        {
            if (!chunk)return;
            GridField<GridObject> buildingGrid = chunk.ChunkBuildingGrid;
            Vector2Int cellPos = buildingGrid.GetCellPosition(mousePosition);
            
            GridObject gridObject = buildingGrid.GetCellData(cellPos);
            if (gridObject == null) return;

            Vector2Int[] positions = ResourcesUtility.GetBuildingDataBase(buildingData).GetGridPositionList(cellPos, facingDirection);
            foreach (Vector2Int position in positions)
            {
                GridObject gridObj = buildingGrid.GetCellData(position);
                if (gridObj == null) continue;
                if(gridObj.Occupied) return;
            }

            PlaceBuilding(chunk, cellPos, buildingData, facingDirection);
        }
        private static void PlaceBuilding(GridChunk chunk, Vector2Int cellPos, PossibleBuildings  buildingData,
            FacingDirection facingDirection)
        {
            Vector2Int[] positions = ResourcesUtility.GetBuildingDataBase(buildingData).GetGridPositionList(cellPos, facingDirection);
            GridField<GridObject> buildGridField = chunk.ChunkBuildingGrid;


            PlacedBuildingEntity building = buildingData switch
            {
                PossibleBuildings.Extractor => PlacedBuildingEntity.CreateBuilding<Extractor>(
                    buildGridField.GetCellData(cellPos),
                    buildGridField.GetWorldPosition(cellPos), cellPos, facingDirection, buildingData),
                PossibleBuildings.Conveyor => PlacedBuildingEntity.CreateBuilding<ConveyorBelt>(
                    buildGridField.GetCellData(cellPos),
                    buildGridField.GetWorldPosition(cellPos), cellPos, facingDirection, buildingData),
                PossibleBuildings.Combiner => PlacedBuildingEntity.CreateBuilding<Combiner>(
                    buildGridField.GetCellData(cellPos),
                    buildGridField.GetWorldPosition(cellPos), cellPos, facingDirection, buildingData),
                PossibleBuildings.TrashCan => PlacedBuildingEntity.CreateBuilding<TrashCan>(
                    buildGridField.GetCellData(cellPos),
                    buildGridField.GetWorldPosition(cellPos), cellPos, facingDirection, buildingData),
                PossibleBuildings.Separator => PlacedBuildingEntity.CreateBuilding<Separator>(
                    buildGridField.GetCellData(cellPos),
                    buildGridField.GetWorldPosition(cellPos), cellPos, facingDirection, buildingData),
                _ => throw new ArgumentOutOfRangeException(nameof(buildingData), buildingData, null)
            };

            foreach (Vector2Int cellPosition in positions)
            {
                if (buildGridField.IsValidPosition(cellPosition))
                { 
                    buildGridField.GetCellData(cellPosition).Occupy(building);
                    buildGridField.TriggerGridObjectChanged(cellPosition);
                }
                else
                {
                   GridChunk otherChunk = chunk.myGridBuildingSystem.GetChunk(GetChunkPositionOverflow(cellPosition, chunk.ChunkPosition));
                   Vector2Int positionOfCell = GetCellPositionOverflow(cellPosition);
                   otherChunk.ChunkBuildingGrid.GetCellData(positionOfCell).Occupy(building);
                   otherChunk.ChunkBuildingGrid.TriggerGridObjectChanged(positionOfCell);
                }
            }

            chunk.Buildings.Add(building);
        }
        public static void TryToDeleteBuilding(GridChunk chunk, Vector3 mousePosition)
        {
            if (!chunk)return;
            GridField<GridObject> buildingGrid = chunk.ChunkBuildingGrid;
            GridObject gridObject = buildingGrid.GetCellData(mousePosition);
            if(!(gridObject is { Occupied: true })) return;
            PlacedBuildingEntity placedBuilding = gridObject.Building;
            chunk.Buildings.Remove(placedBuilding);
            placedBuilding.Destroy();
                    
            foreach (Vector2Int occupiedCell in placedBuilding.GetGridPositionList())
            {
                buildingGrid.GetCellData(occupiedCell).ClearBuilding();
            }
        }

        public static Vector2Int GetChunkPositionOverflow(Vector2Int pseudoCellPosition, Vector2Int chunkPosition)
        {
            if (pseudoCellPosition.x < 0) chunkPosition.x -= 1;
            else if (pseudoCellPosition.x >= GridBuildingSystem.ChunkSize.x) chunkPosition.x += 1;
            if (pseudoCellPosition.y < 0) chunkPosition.y -= 1;
            else if (pseudoCellPosition.y >= GridBuildingSystem.ChunkSize.y) chunkPosition.y += 1;
            return chunkPosition;
        }
        
        public static Vector2Int GetCellPositionOverflow(Vector2Int pseudoCellPosition)
        {
            if (pseudoCellPosition.x < 0) pseudoCellPosition.x += GridBuildingSystem.ChunkSize.x;
            else if (pseudoCellPosition.x >= GridBuildingSystem.ChunkSize.x) pseudoCellPosition.x -= GridBuildingSystem.ChunkSize.x;
            if (pseudoCellPosition.y < 0) pseudoCellPosition.y += GridBuildingSystem.ChunkSize.y;
            else if (pseudoCellPosition.y >= GridBuildingSystem.ChunkSize.y) pseudoCellPosition.y -= GridBuildingSystem.ChunkSize.y;
            return pseudoCellPosition;
        }

        public void Load(List<Vector2Int> loadedChunks)
        {
            if(Loaded) return;
            Loaded = true;
            chunkTilemapRenderer.enabled = Loaded;
            if(!loadedChunks.Contains(ChunkPosition)) loadedChunks.Add(ChunkPosition);
        }
        
        public void UnLoad(List<Vector2Int> loadedChunks)
        {
            if(!Loaded) return;
            Loaded = false;
            chunkTilemapRenderer.enabled = Loaded;
            if(loadedChunks.Contains(ChunkPosition))loadedChunks.Remove(ChunkPosition);
        }
    }
}

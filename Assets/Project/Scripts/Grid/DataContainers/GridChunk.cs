using System;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Buildings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Project.Scripts.Grid.DataContainers
{
    public class GridChunk : MonoBehaviour
    {
        public GridField<GridObject> BuildingGrid { get; private set; }
        public List<PlacedBuildingData> BuildingsData { get; private set; } = new List<PlacedBuildingData>();
        public Tilemap ChunkTilemap { get; private set; }
        public List<ChunkResourcePoint> ChunkResources { get; } = new List<ChunkResourcePoint>();

        private TilemapRenderer chunkTilemapRenderer;
        public Vector3 LocalPosition { get; private set; }
        public Vector2Int ChunkPosition { get; private set; }
        public bool Loaded { get; private set; } = true;

        public void Initialization(Vector2Int chunkPosition, Vector3 localPosition, PlacedBuildingData[] buildings = null, ChunkResourcePoint[] resourcePoints = null)
        {
            ChunkPosition = chunkPosition;
            LocalPosition = localPosition;
            transform.localPosition = LocalPosition;
            ChunkTilemap = GetComponentInChildren<Tilemap>();
            chunkTilemapRenderer = GetComponentInChildren<TilemapRenderer>();
            BuildingGrid = new GridField<GridObject>(GridBuildingSystem.GridSize,GridBuildingSystem.CellSize, transform,
                (field, pos) => new GridObject(this, pos));

            if (resourcePoints != null)
            {
                foreach (ChunkResourcePoint resourcePoint in resourcePoints)
                {
                    StartCoroutine(JobSetResourceCell(resourcePoint));
                }
            }
            
            if(buildings == null) BuildingsData = new List<PlacedBuildingData>();
            else
            {
                foreach (PlacedBuildingData building in buildings)
                {
                    StartCoroutine(JobPlaceBuilding(this, building.origin,
                        building.buildingData, building.direction));
                }
            }
        }

        private IEnumerator JobSetResourceCell( ChunkResourcePoint resourcePoint)
        {
            BuildingGrid.GetCellData(resourcePoint.position).SetResource(resourcePoint.resource);
            yield return null;
        }

        private IEnumerator JobPlaceBuilding(GridChunk chunk, Vector2Int cellPos, BuildingGridResources.PossibleBuildings buildingData,
            BuildingDataBase.Directions direction)
        {
            PlaceBuilding(chunk,cellPos,buildingData,direction);
            yield return null;
        }

        public static void TryToPlaceBuilding( GridChunk chunk,BuildingGridResources.PossibleBuildings  buildingData, Vector3 mousePosition,
            BuildingDataBase.Directions direction)
        {
            if (!chunk)return;
            GridField<GridObject> buildingGrid = chunk.BuildingGrid;
            Vector2Int cellPos = buildingGrid.GetCellPosition(mousePosition);
            
            GridObject gridObject = buildingGrid.GetCellData(cellPos);
            if (gridObject == null) return;

            Vector2Int[] positions = BuildingGridResources.GetBuildingDataBase(buildingData).GetGridPositionList(cellPos, direction);
            foreach (Vector2Int position in positions)
            {
                GridObject gridObj = buildingGrid.GetCellData(position);
                if (gridObj == null || gridObj.Occupied) return;
            }

            PlaceBuilding(chunk, cellPos, buildingData, direction);
        }
        private static void PlaceBuilding(GridChunk chunk, Vector2Int cellPos, BuildingGridResources.PossibleBuildings  buildingData,
            BuildingDataBase.Directions direction)
        {
            Vector2Int[] positions = BuildingGridResources.GetBuildingDataBase(buildingData).GetGridPositionList(cellPos, direction);
            PlacedBuilding building = PlacedBuilding.CreateBuilding(
                chunk.BuildingGrid.GetLocalPosition(cellPos), cellPos, direction, buildingData,
                chunk.transform, chunk.BuildingGrid.CellSize);

            foreach (Vector2Int blockedCell in positions)
            {
                chunk.BuildingGrid.GetCellData(blockedCell).Occupy(building);
                chunk.BuildingGrid.TriggerGridObjectChanged(blockedCell);
            }

            chunk.BuildingsData.Add(building.PlacedBuildingData);
        }
        public static void TryToDeleteBuilding(GridChunk chunk, Vector3 mousePosition)
        {
            if (!chunk)return;
            GridField<GridObject> buildingGrid = chunk.BuildingGrid;
            GridObject gridObject = buildingGrid.GetCellData(mousePosition);
            if(!(gridObject is { Occupied: true })) return;
            PlacedBuilding placedBuilding = gridObject.Building;
            placedBuilding.Destroy();
                    
            foreach (Vector2Int occupiedCell in placedBuilding.GetGridPositionList())
            {
                buildingGrid.GetCellData(occupiedCell).ClearBuilding();
            }
        }

        public void Load()
        {
            if(Loaded) return;
            Loaded = true;
            chunkTilemapRenderer.enabled = Loaded;
        }
        
        public void UnLoad()
        {
            if(!Loaded) return;
            Loaded = false;
            chunkTilemapRenderer.enabled = Loaded;
        }
    }

    [Serializable]
    public class ChunkSave
    {
        public ChunkResourcePoint[] chunkResourcePoints;
        public PlacedBuildingData[] placedBuildingData;
        public Vector3 localPosition;
        public Vector2Int chunkPosition;
    }
    
    [Serializable]
    public struct ChunkResourcePoint
    {
        public Vector2Int position;
        public BuildingGridResources.ResourcesType resource;
    }
}

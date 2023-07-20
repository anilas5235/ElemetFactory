using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Buildings;
using Project.Scripts.Grid.CellType;
using Project.Scripts.Interaction;
using Project.Scripts.Utilities;
using Project.Scripts.Visualisation;
using UnityEngine;

namespace Project.Scripts.Grid
{
    /*
     * Code based on the work of Code Monkey
     */
    [RequireComponent(typeof(UnityEngine.Grid))]
    public class GridBuildingSystem : MonoBehaviour
    {
        [SerializeField] private List<BuildingDataBase> buildings;
        private BuildingDataBase _selectedBuilding;
        private BuildingDataBase.Directions _direction = BuildingDataBase.Directions.Down;

        public static readonly Vector2Int GridSize = new Vector2Int(10, 10);
        public const float CellSize = 10f;
        public static readonly Vector2 ChunkSize = new Vector2(CellSize * GridSize.x, CellSize * GridSize.y);
        [SerializeField] private GameObject chunkPrefap;

        private Dictionary<Vector2Int, GridChunk> Chunks { get; } = new Dictionary<Vector2Int, GridChunk>();
        private List<Vector2Int> LoadedChunks { get; } = new List<Vector2Int>();

        private Camera PlayerCam => CameraMovement.Instance.Camera;

        private void Awake()
        {
            GetComponent<UnityEngine.Grid>().cellSize = new Vector3(CellSize, CellSize, 0);
            _selectedBuilding = buildings.First();
            InitialChunks();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                _direction = BuildingDataBase.GetNextDirection(_direction);
                Debug.Log($"rotation: {_direction}");
            }

            if (Input.GetMouseButton(0) && _selectedBuilding)
            {
                Vector3 mousePos =GeneralUtilities.GetMousePosition();
                TryToPlaceBuilding(GetChunk(mousePos),_selectedBuilding,mousePos,_direction);
            }

            if (Input.GetMouseButton(1))
            {
                Vector3 mousePos =GeneralUtilities.GetMousePosition();
                TryToDeleteBuilding(GetChunk(mousePos));
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) { _selectedBuilding = buildings[0]; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { _selectedBuilding = buildings[1]; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { _selectedBuilding = buildings[2]; }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { _selectedBuilding = buildings[3]; }

        }

        private void FixedUpdate()
        {
            Vector2Int chunkPosWithPlayer = GetChunkPosition(PlayerCam.transform.position);
            int playerViewRadius = Mathf.CeilToInt(PlayerCam.orthographicSize * PlayerCam.aspect / ChunkSize.x);

            List<Vector2Int> chunksToLoad = new List<Vector2Int>();
            List<Vector2Int> chunksToUnLoad = new List<Vector2Int>();
            
            for (int x = -playerViewRadius; x < playerViewRadius+1; x++)
            {
                for (int y = -playerViewRadius; y < playerViewRadius+1; y++)
                { 
                    chunksToLoad.Add(new Vector2Int(x, y) + chunkPosWithPlayer);
                }
            }

            foreach (Vector2Int loadedChunkPos in LoadedChunks)
            {
                if (chunksToLoad.Contains(loadedChunkPos)) chunksToLoad.Remove(loadedChunkPos);
                else chunksToUnLoad.Add(loadedChunkPos);
            }
            
            foreach (Vector2Int chunkPos in chunksToLoad) LoadChunk(chunkPos);
            foreach (Vector2Int chunkPos in chunksToUnLoad) UnLoadChunk(chunkPos);
        }

        #region Build
        public static void TryToPlaceBuilding( GridChunk chunk,BuildingDataBase buildingData, Vector3 mousePosition,
            BuildingDataBase.Directions direction)
        {
            if (!chunk)return;
            GridField<GridObject> buildingGrid = chunk.BuildingGrid;
            Vector2Int cellPos = buildingGrid.GetCellPosition(mousePosition);
            
            GridObject gridObject = buildingGrid.GetCellData(cellPos);
            if (gridObject == null) return;

            List<Vector2Int> positions = buildingData.GetGridPositionList(cellPos, direction);
            foreach (Vector2Int position in positions)
            {
                GridObject gridObj = buildingGrid.GetCellData(position);
                if (gridObj == null || gridObj.Occupied) return;
            }

            PlacedBuilding building = PlacedBuilding.CreateBuilding(
                buildingGrid.GetLocalPosition(cellPos),
                cellPos, positions.ToArray(), direction, buildingData, chunk.transform,
                buildingGrid.CellSize);
            
            foreach (Vector2Int blockedCell in positions)
            {
                buildingGrid.GetCellData(blockedCell).Occupy(building);
                buildingGrid.TriggerGridObjectChanged(blockedCell);
            }

            chunk.Buildings.Add(building);
        }

        private static void TryToDeleteBuilding(GridChunk chunk)
        {
            if (!chunk)return;
            GridField<GridObject> buildingGrid = chunk.BuildingGrid;
            GridObject gridObject = buildingGrid.GetCellData(GeneralUtilities.GetMousePosition());
            if(!(gridObject is { Occupied: true })) return;
            PlacedBuilding placedBuilding = gridObject.Building;
            placedBuilding.Destroy();
                    
            foreach (Vector2Int occupiedCell in placedBuilding.OccupiedCells)
            {
                buildingGrid.GetCellData(occupiedCell).ClearBuilding();
            }
        }
        #endregion

        #region ChunkSystem

        public void LoadChunk(Vector2Int position)
        {
            GridChunk targetChunk = GetChunk(position);
            targetChunk.Load();
            if(!LoadedChunks.Contains(position)) LoadedChunks.Add(position);
        }
        
        public void UnLoadChunk(Vector2Int position)
        {
            GridChunk targetChunk = GetChunk(position);
            targetChunk.UnLoad();
            LoadedChunks.Remove(position);
        }

        public static Vector2Int GetChunkPosition(Vector3 worldPosition)
        {
            return new Vector2Int(Mathf.RoundToInt(worldPosition.x / ChunkSize.x),
                Mathf.RoundToInt(worldPosition.y / ChunkSize.y));
        }

        public GridChunk GetChunk(Vector2Int chunkPosition)
        {
            return Chunks.TryGetValue(chunkPosition, out var chunk) ? chunk : CreateChunk(chunkPosition);
        }
        
        public GridChunk GetChunk(Vector3 worldPosition)
        {
            return GetChunk(GetChunkPosition(worldPosition));
        }

        private void InitialChunks()
        {
            Vector2Int size = new Vector2Int(10, 10);
            Vector2Int half = new Vector2Int(Mathf.RoundToInt(size.x / 2f), Mathf.RoundToInt(size.y / 2f));
            
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int position = new Vector2Int(x - half.x, y-half.y);
                    CreateChunk(position);
                }
            }
        }

        private GridChunk CreateChunk(Vector2Int chunkPosition)
        {
            GridChunk newChunk = Instantiate(chunkPrefap,transform).GetComponent<GridChunk>();
            Vector3 localPosition = new Vector3(chunkPosition.x * ChunkSize.x, chunkPosition.y * ChunkSize.y);
            newChunk.Initialization(chunkPosition, localPosition);
            newChunk.gameObject.name = $"Chunk {chunkPosition}";
            Chunks.Add(chunkPosition, newChunk);
            LoadedChunks.Add(chunkPosition);
            return newChunk;
        }
        #endregion
    }

    public class GridObject
    {
        private GridField<GridObject> GridField => Chunk.BuildingGrid;
        public Vector2Int Position { get; }
        public bool Occupied => Building;
        public PlacedBuilding Building { get; private set; }
        public CellResources ResourceNode { get; }
        public GridChunk Chunk { get; }

        public GridObject(GridChunk chunk, Vector2Int position,  CellResources resource = null)
        {
            Chunk = chunk;
            Position = position;
            ResourceNode = resource;

            if (ResourceNode != null)
            {
                Vector3Int tilePos = new Vector3Int(
                    Mathf.FloorToInt(Position.x - GridBuildingSystem.GridSize.x / 2f),
                    Mathf.FloorToInt(Position.y - GridBuildingSystem.GridSize.y / 2f),
                    0);
                Chunk.ChunkTilemap.SetTile(tilePos, VisualResources.GetTileSource(ResourceNode.NodeType));
            }
        }

        #region Functions
        public void Occupy(PlacedBuilding building)
        {
            if (Building) return;
            Building = building;
            GridField.TriggerGridObjectChanged(Position);
        }

        public void ClearBuilding()
        {
            if (!Building) return;
            Building = null;
            GridField.TriggerGridObjectChanged(Position);
        }

        public override string ToString()
        {
            return Position + "\n" + Building + "\n" + ResourceNode;
        }
        #endregion
    }
}
